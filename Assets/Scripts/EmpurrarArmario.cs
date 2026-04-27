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
    private Vector3 distanciaInicial; // A variável mágica que resolve o problema!

    void Update()
    {
        if (Keyboard.current == null) return;

        // Ao carregar no E, tenta agarrar ou larga
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
            // 1. Calcula onde o armário DEVE estar (a mão + a distância original)
            Vector3 posicaoAlvo = posicaoSegurar.position + distanciaInicial;

            // 2. Descobre a direção para ir para esse ponto alvo
            Vector3 direcao = posicaoAlvo - armarioSegurado.position;

            // 3. Prende ao chão (Impede que levante voo)
            direcao.y = 0;

            // 4. Aplica a força
            armarioSegurado.linearVelocity = direcao * forcaArrastar;
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
                    // Baixamos o atrito (Linear Damping) para ele deslizar
                    armarioSegurado.linearDamping = 5f;

                    // A MAGIA: Guarda a distância exata entre a mão e o armário no momento do clique!
                    distanciaInicial = armarioSegurado.position - posicaoSegurar.position;
                    distanciaInicial.y = 0; // Ignora diferenças de altura
                }
            }
        }
    }

    void Largar()
    {
        if (armarioSegurado != null)
        {
            // Devolvemos o atrito a 100 para ele travar a fundo
            armarioSegurado.linearDamping = 100f;
            armarioSegurado = null;
        }
    }
}