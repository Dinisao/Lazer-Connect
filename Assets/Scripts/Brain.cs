using UnityEngine;
using UnityEngine.InputSystem;

public class InteracaoJogador : MonoBehaviour
{
    [Header("Configurações de Distância")]
    public float distanciaInteracao = 4f;
    public float distanciaColagem = 3f;

    [Header("Referências")]
    public Transform pontoParaSegurar;
    public GameObject textoAviso;

    [Header("Física")]
    public float forcaSeguir = 15f;

    private GameObject objetoSegurado;
    private Rigidbody rbSegurado;

    void Update()
    {
        // Só mostra o aviso visual se não estivermos a carregar nada
        if (objetoSegurado == null)
            VerificarMira();
        else if (textoAviso != null)
            textoAviso.SetActive(false);

        // Tecla E para interagir (Agarrar ou Largar/Colar)
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (objetoSegurado == null) TentarInteragir();
            else LargarEColar();
        }
    }

    void FixedUpdate()
    {
        // A correção para o aviso: 
        // Só move se o objeto não for Kinematic
        if (objetoSegurado != null && rbSegurado != null && !rbSegurado.isKinematic)
        {
            MoverObjetoComFisica();
        }
    }

    void MoverObjetoComFisica()
    {
        Vector3 direcao = pontoParaSegurar.position - objetoSegurado.transform.position;
        float distancia = direcao.magnitude;

        // Aplica a velocidade para o objeto seguir a mão
        rbSegurado.linearVelocity = direcao * forcaSeguir;

        // Suaviza a rotação para acompanhar a visão do jogador
        Quaternion rotacaoAlvo = transform.rotation;
        rbSegurado.MoveRotation(Quaternion.Lerp(rbSegurado.rotation, rotacaoAlvo, Time.fixedDeltaTime * 10f));

        // Se o objeto ficar preso atrás de uma parede, larga-o automaticamente
        if (distancia > 2.5f) LargarEColar();
    }

    void TentarInteragir()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao))
        {
            // Interação com o Botão do Laser
            if (hit.collider.CompareTag("Button"))
            {
                ControloLaser laser = hit.collider.GetComponentInParent<ControloLaser>();
                if (laser != null) laser.estaLigado = !laser.estaLigado;
            }

            // Agarrar o Espelho
            if (hit.collider.CompareTag("Mirror"))
            {
                objetoSegurado = hit.collider.gameObject;
                rbSegurado = objetoSegurado.GetComponent<Rigidbody>();

                if (rbSegurado != null)
                {
                    rbSegurado.useGravity = false;
                    rbSegurado.isKinematic = false; // Permite movimento físico
                    rbSegurado.linearDamping = 10f;
                    rbSegurado.angularDamping = 10f;
                }
            }
        }
    }

    void LargarEColar()
    {
        if (objetoSegurado == null) return;

        RaycastHit hit;
        // Tenta detetar uma parede para colar o espelho
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaColagem))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                rbSegurado.isKinematic = true; // Congela o objeto e para os avisos
                rbSegurado.useGravity = false;
                rbSegurado.linearVelocity = Vector3.zero;

                // Posiciona o espelho fora da parede para não entrar nela
                objetoSegurado.transform.position = hit.point + (hit.normal * 0.4f);

                // Snap de 45 graus para reflexão perfeita do laser
                float anguloY = Mathf.Round(transform.eulerAngles.y / 45f) * 45f;
                objetoSegurado.transform.rotation = Quaternion.Euler(0, anguloY, 0);
            }
            else { SoltarFisicamente(); }
        }
        else { SoltarFisicamente(); }

        objetoSegurado = null;
        rbSegurado = null;
    }

    void SoltarFisicamente()
    {
        rbSegurado.useGravity = true;
        rbSegurado.isKinematic = false;
        rbSegurado.linearDamping = 0.05f;
        rbSegurado.angularDamping = 0.05f;

        // Mantém o snap de ângulo mesmo ao cair no chão
        float anguloY = Mathf.Round(objetoSegurado.transform.eulerAngles.y / 45f) * 45f;
        objetoSegurado.transform.rotation = Quaternion.Euler(0, anguloY, 0);
    }

    void VerificarMira()
    {
        if (textoAviso == null) return;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao))
        {
            if (hit.collider.CompareTag("Button") || hit.collider.CompareTag("Mirror"))
            {
                textoAviso.SetActive(true);
                return;
            }
        }
        textoAviso.SetActive(false);
    }
}