using UnityEngine;
using UnityEngine.InputSystem;

public class InteracaoJogador : MonoBehaviour
{
    public float distanciaInteracao = 4f;
    public float distanciaColagem = 3f;
    public Transform pontoParaSegurar;
    public GameObject textoAviso;
    public float forcaSeguir = 15f;
    public float offsetRotacaoY = 180f;

    private GameObject objetoSegurado;
    private Rigidbody rbSegurado;

    void Update()
    {
        if (objetoSegurado == null) VerificarMira();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (objetoSegurado == null) TentarInteragir();
            else LargarEColar();
        }

        // RODAR: Funciona para o que tens na mão OU o que olhas
        if (Keyboard.current.rKey.wasPressedThisFrame) TentarRodar();
    }

    void FixedUpdate()
    {
        if (objetoSegurado != null && rbSegurado != null && !rbSegurado.isKinematic)
        {
            Vector3 direcao = pontoParaSegurar.position - objetoSegurado.transform.position;
            rbSegurado.linearVelocity = direcao * forcaSeguir;

            Quaternion rotacaoAlvo = transform.rotation * Quaternion.Euler(0, offsetRotacaoY, 0);
            rbSegurado.MoveRotation(Quaternion.Lerp(rbSegurado.rotation, rotacaoAlvo, Time.fixedDeltaTime * 10f));
        }
    }

    void TentarInteragir()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao))
        {
            if (hit.collider.CompareTag("Button"))
            {
                ControloLaser laser = Object.FindFirstObjectByType<ControloLaser>();
                if (laser != null) laser.AlternarLaser();
                return;
            }

            if (hit.collider.CompareTag("Mirror"))
            {
                rbSegurado = hit.collider.GetComponent<Rigidbody>() ?? hit.collider.GetComponentInParent<Rigidbody>();
                if (rbSegurado != null)
                {
                    objetoSegurado = rbSegurado.gameObject;
                    rbSegurado.isKinematic = false;
                    rbSegurado.useGravity = false;
                }
            }
        }
    }

    void LargarEColar()
    {
        if (objetoSegurado == null) return;
        RaycastHit hit;
        // Offset de 0.6f para garantir que ao rodar o espelho não atravessa a parede
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaColagem) && hit.collider.CompareTag("Wall"))
        {
            rbSegurado.isKinematic = true;
            rbSegurado.linearVelocity = Vector3.zero;
            objetoSegurado.transform.position = hit.point + (hit.normal * 0.6f);
            objetoSegurado.transform.rotation = Quaternion.LookRotation(hit.normal);
        }
        else { rbSegurado.isKinematic = false; rbSegurado.useGravity = true; }
        objetoSegurado = null; rbSegurado = null;
    }

void TentarRodar()
{
    // 1. Se estivermos a segurar algo, rodamos esse objeto diretamente
    if (objetoSegurado != null) 
    {
        MirrorRotate rot = objetoSegurado.GetComponent<MirrorRotate>() ?? objetoSegurado.GetComponentInParent<MirrorRotate>();
        if (rot != null) 
        {
            rot.Rotate();
            // Debug para termos a certeza que o código está a chegar aqui
            Debug.Log("Rodei o espelho na mão!");
        }
        return; 
    }

    // 2. Se não estivermos a segurar nada, tentamos rodar o que o jogador está a olhar
    RaycastHit hit;
    if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao))
    {
        if (hit.collider.CompareTag("Mirror") || hit.collider.CompareTag("FixedMirror"))
        {
            MirrorRotate rot = hit.collider.GetComponent<MirrorRotate>() ?? hit.collider.GetComponentInParent<MirrorRotate>();
            if (rot != null) rot.Rotate();
        }
    }
}

    void VerificarMira()
    {
        if (textoAviso == null) return;
        RaycastHit hit;
        bool mirando = Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao) &&
                       (hit.collider.CompareTag("Button") || hit.collider.CompareTag("Mirror") || hit.collider.CompareTag("FixedMirror"));
        textoAviso.SetActive(mirando);
    }
}