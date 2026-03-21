using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InteracaoJogador : MonoBehaviour
{
    [Header("Configurações de Distância")]
    public float distanciaInteracao = 5f;
    public float distanciaColagem = 4f;
    public float offsetColagem = 0.45f;

    [Header("Referências")]
    public Transform pontoParaSegurar;
    public GameObject textoAviso;
    public float forcaSeguir = 25f;

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

        if (Keyboard.current.rKey.wasPressedThisFrame) TentarRodar();
    }

    void FixedUpdate()
    {
        if (objetoSegurado != null && rbSegurado != null)
        {
            Vector3 proximaPosicao = Vector3.Lerp(rbSegurado.position, pontoParaSegurar.position, Time.fixedDeltaTime * forcaSeguir);
            rbSegurado.MovePosition(proximaPosicao);

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
            bool ehMirror = hit.collider.CompareTag("Mirror") || (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Mirror"));

            if (rb != null && ehMirror)
            {
                rbSegurado = rb;
                objetoSegurado = rb.gameObject;

                // --- SOLUÇÃO RADICAL ---
                // Desliga todos os colliders do espelho. Se não há collider, não há empurrão.
                Collider[] colls = objetoSegurado.GetComponentsInChildren<Collider>();
                foreach (Collider c in colls) c.enabled = false;

                rbSegurado.isKinematic = true;
                rbSegurado.useGravity = false;
                rbSegurado.linearVelocity = Vector3.zero;
                rbSegurado.angularVelocity = Vector3.zero;

                objetoSegurado.transform.SetParent(null);
            }
            else if (hit.collider.CompareTag("Button"))
            {
                var laser = Object.FindFirstObjectByType<ControloLaser>();
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
                GameObject obj = objetoSegurado;
                Rigidbody rb = rbSegurado;

                StartCoroutine(ForcarPosicaoNaParede(obj, rb, hit.point, hit.normal));

                objetoSegurado = null;
                rbSegurado = null;
                return;
            }
        }

        // Se largar no chão, religa os colliders para ele não atravessar o mapa
        AtivarColliders(objetoSegurado);

        rbSegurado.isKinematic = false;
        rbSegurado.useGravity = true;
        objetoSegurado = null;
        rbSegurado = null;
    }

    void AtivarColliders(GameObject obj)
    {
        if (obj == null) return;
        Collider[] colls = obj.GetComponentsInChildren<Collider>();
        foreach (Collider c in colls) c.enabled = true;
    }

    IEnumerator ForcarPosicaoNaParede(GameObject obj, Rigidbody rb, Vector3 ponto, Vector3 normal)
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;

        obj.transform.position = ponto + (normal * offsetColagem);
        obj.transform.rotation = Quaternion.LookRotation(normal);

        yield return new WaitForFixedUpdate();

        // Religa os colliders depois de estar colado na parede
        AtivarColliders(obj);
    }

    void TentarRodar()
    {
        GameObject alvo = (objetoSegurado != null) ? objetoSegurado : null;
        if (alvo == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao))
            {
                MirrorRotate rot = hit.collider.GetComponentInParent<MirrorRotate>() ?? hit.collider.GetComponentInChildren<MirrorRotate>();
                if (rot != null) rot.Rotate();
            }
        }
        else
        {
            MirrorRotate rot = alvo.GetComponent<MirrorRotate>() ?? alvo.GetComponentInChildren<MirrorRotate>();
            if (rot != null) rot.Rotate();
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