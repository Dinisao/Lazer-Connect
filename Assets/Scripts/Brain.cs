using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InteracaoJogador : MonoBehaviour
{
    [Header("Configurações")]
    public float distanciaInteracao = 5f;
    public float distanciaColagem = 4f;
    public float offsetColagem = 0.45f;
    public Transform pontoParaSegurar;
    public GameObject textoAviso;
    public float forcaSeguir = 25f;

    private GameObject objetoSegurado;
    private Rigidbody rbSegurado;

    void Update()
    {
        if (objetoSegurado == null) VerificarMira();

        // Interagir / Largar (Tecla E)
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (objetoSegurado == null) TentarInteragir();
            else LargarEColar();
        }

        // Rodar (Tecla R)
        if (Keyboard.current.rKey.wasPressedThisFrame) TentarRodar();
    }

    void FixedUpdate()
    {
        if (objetoSegurado != null && rbSegurado != null)
        {
            Vector3 proximaPosicao = Vector3.Lerp(rbSegurado.position, pontoParaSegurar.position, Time.fixedDeltaTime * forcaSeguir);
            rbSegurado.MovePosition(proximaPosicao);

            // Mantém o espelho virado para a frente do jogador enquanto segura
            Quaternion proximaRotacao = pontoParaSegurar.rotation * Quaternion.Euler(0, 180, 0);
            rbSegurado.MoveRotation(Quaternion.Slerp(rbSegurado.rotation, proximaRotacao, Time.fixedDeltaTime * forcaSeguir));
        }
    }

    void TentarInteragir()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao))
        {
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();
            bool ehMirror = hit.collider.CompareTag("Mirror") ||
                           (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Mirror"));

            if (rb != null && ehMirror)
            {
                rbSegurado = rb;
                objetoSegurado = rb.gameObject;

                // ANTI-VOO: Desativa colliders enquanto segura
                Collider[] colls = objetoSegurado.GetComponentsInChildren<Collider>();
                foreach (Collider c in colls) c.enabled = false;

                rbSegurado.isKinematic = true;
                rbSegurado.useGravity = false;
            }
            else if (hit.collider.CompareTag("Button"))
            {
                ControloLaser laser = hit.collider.GetComponentInParent<ControloLaser>();
                if (laser == null) laser = Object.FindFirstObjectByType<ControloLaser>();
                if (laser != null) laser.AlternarLaser();
            }
        }
    }

    void LargarEColar()
    {
        if (objetoSegurado == null) return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaColagem))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                StartCoroutine(ForcarPosicaoNaParede(objetoSegurado, rbSegurado, hit.point, hit.normal));
                objetoSegurado = null;
                rbSegurado = null;
                return;
            }
        }

        FinalizarSoltar(objetoSegurado, rbSegurado);
        objetoSegurado = null;
        rbSegurado = null;
    }

    IEnumerator ForcarPosicaoNaParede(GameObject obj, Rigidbody rb, Vector3 pontoImpacto, Vector3 normalParede)
    {
        rb.isKinematic = true;
        Vector3 posFinal = pontoImpacto + (normalParede * offsetColagem);
        Quaternion rotFinal = Quaternion.LookRotation(normalParede, Vector3.up);

        for (int i = 0; i < 5; i++)
        {
            if (obj == null) break;
            obj.transform.position = posFinal;
            obj.transform.rotation = rotFinal;
            yield return new WaitForFixedUpdate();
        }

        // Reativa colliders após colar
        Collider[] colls = obj.GetComponentsInChildren<Collider>();
        foreach (Collider c in colls) c.enabled = true;
    }

    void FinalizarSoltar(GameObject obj, Rigidbody rb)
    {
        if (obj == null) return;
        Collider[] colls = obj.GetComponentsInChildren<Collider>();
        foreach (Collider c in colls) c.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    void TentarRodar()
    {
        // SE ESTIVER A SEGURAR: Roda o objeto que tem na mão diretamente
        if (objetoSegurado != null)
        {
            MirrorRotate rot = objetoSegurado.GetComponent<MirrorRotate>() ?? objetoSegurado.GetComponentInChildren<MirrorRotate>();
            if (rot != null) rot.Rotate();
        }
        // SE NÃO ESTIVER A SEGURAR: Usa Raycast para rodar espelhos fixos ou na parede
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao))
            {
                MirrorRotate rot = hit.collider.GetComponentInParent<MirrorRotate>() ?? hit.collider.GetComponentInChildren<MirrorRotate>();
                if (rot != null) rot.Rotate();
            }
        }
    }

    void VerificarMira()
    {
        if (textoAviso == null) return;
        RaycastHit hit;
        bool mirando = Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao) &&
                       (hit.collider.CompareTag("Mirror") || hit.collider.CompareTag("Button"));
        textoAviso.SetActive(mirando);
    }
}