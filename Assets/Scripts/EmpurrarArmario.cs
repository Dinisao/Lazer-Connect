using UnityEngine;
using UnityEngine.InputSystem;

public class EmpurrarArmario : MonoBehaviour
{
    [Header("Configurações")]
    public float distanciaPegar = 3.5f;
    public float forcaArrastar = 15f;

    [Header("Referências")]
    public Camera cameraJogador;
    public Transform posicaoSegurar;

    private Rigidbody armarioSegurado;
    private Vector3 distanciaInicial;
    private Vector3 eixoDeMovimento;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (armarioSegurado == null) TentarAgarrar();
            else Largar();
        }
    }

    void FixedUpdate()
    {
        if (armarioSegurado != null)
        {
            Vector3 posicaoAlvo = posicaoSegurar.position + distanciaInicial;
            Vector3 direcaoLivre = posicaoAlvo - armarioSegurado.position;
            direcaoLivre.y = 0;

            // Filtra o movimento pelo carril exato da face tocada
            Vector3 direcaoRestrita = Vector3.Project(direcaoLivre, eixoDeMovimento);

            armarioSegurado.linearVelocity = direcaoRestrita * forcaArrastar;
        }
    }

    void TentarAgarrar()
    {
        RaycastHit hit;

        if (Physics.Raycast(cameraJogador.transform.position, cameraJogador.transform.forward, out hit, distanciaPegar))
        {
            if (hit.transform.CompareTag("Armario"))
            {
                armarioSegurado = hit.transform.GetComponent<Rigidbody>();

                if (armarioSegurado != null)
                {
                    armarioSegurado.linearDamping = 5f;

                    distanciaInicial = armarioSegurado.position - posicaoSegurar.position;
                    distanciaInicial.y = 0;

                    // A MAGIA DEFINITIVA: hit.normal!
                    // Em vez de confiar no modelo 3D (que pode estar estragado), 
                    // criamos o carril com base na face exata onde o teu raio bateu.
                    eixoDeMovimento = hit.normal;
                    eixoDeMovimento.y = 0;
                    eixoDeMovimento.Normalize();
                }
            }
        }
    }

    void Largar()
    {
        if (armarioSegurado != null)
        {
            armarioSegurado.linearDamping = 100f;
            armarioSegurado = null;
        }
    }
}