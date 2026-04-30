using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InteracaoJogador : MonoBehaviour
{
    [Header("Configurações")]
    public float distanciaInteracao = 5f;
    public float distanciaColagem = 4f;
    public float offsetColagem = 0.45f;

    [Header("Ajuste Fino na Mão")]
    [Tooltip("Usa o Z para afastar o espelho da cara, o Y para o baixar, e o X para os lados.")]
    public Vector3 offsetSegurar = new Vector3(0, 0, 0);

    [Header("SISTEMA DE GRELHA (Snap Parede)")]
    [Tooltip("O tamanho dos quadrados da parede. (Ex: 1, 2 ou 4)")]
    public float tamanhoGrelha = 1f;
    [Tooltip("Se o espelho colar em cima da linha em vez de no meio, põe 0.5 nos eixos aqui!")]
    public Vector3 offsetGrelha = new Vector3(0, 0, 0);

    [Header("Referências")]
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
            // Calcula a posição perfeita com o teu Ajuste Fino
            Vector3 posicaoAlvo = pontoParaSegurar.position +
                                  pontoParaSegurar.right * offsetSegurar.x +
                                  pontoParaSegurar.up * offsetSegurar.y +
                                  pontoParaSegurar.forward * offsetSegurar.z;

            Vector3 proximaPosicao = Vector3.Lerp(rbSegurado.position, posicaoAlvo, Time.fixedDeltaTime * forcaSeguir);
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

            // Aceita se a Tag estiver no próprio colisor (HitboxVidro) ou no Pai
            bool ehMirror = hit.collider.CompareTag("Mirror") ||
                            (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Mirror"));

            if (rb != null && ehMirror)
            {
                rbSegurado = rb;
                objetoSegurado = rb.gameObject;

                // ANTI-VOO: Desativa a Hitbox e qualquer outro collider enquanto segura
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

        // --- MAGIA DO SNAP (ENCAIXE MAGNÉTICO) ---
        Vector3 pontoSnap = pontoImpacto;

        // Função matemática que arredonda para o quadrado mais próximo
        float Arredondar(float valor, float tamanho, float offset)
        {
            if (tamanho == 0) return valor; // Previne erros caso ponhas 0 no inspector sem querer
            return Mathf.Round((valor - offset) / tamanho) * tamanho + offset;
        }

        // Descobre que face da parede é esta, para NÃO arredondar a profundidade (Senão afundava)
        if (Mathf.Abs(normalParede.x) < 0.5f) pontoSnap.x = Arredondar(pontoSnap.x, tamanhoGrelha, offsetGrelha.x);
        if (Mathf.Abs(normalParede.y) < 0.5f) pontoSnap.y = Arredondar(pontoSnap.y, tamanhoGrelha, offsetGrelha.y);
        if (Mathf.Abs(normalParede.z) < 0.5f) pontoSnap.z = Arredondar(pontoSnap.z, tamanhoGrelha, offsetGrelha.z);

        // Aplica o encaixe mais o afastamento da parede
        Vector3 posFinal = pontoSnap + (normalParede * offsetColagem);
        Quaternion rotFinal = Quaternion.LookRotation(normalParede, Vector3.up);

        for (int i = 0; i < 5; i++)
        {
            if (obj == null) break;
            obj.transform.position = posFinal;
            obj.transform.rotation = rotFinal;
            yield return new WaitForFixedUpdate();
        }

        // Reativa hitboxes após colar para o laser bater
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

        // Agora também reage quer estejas a olhar para a Hitbox ou para o corpo principal
        bool olhandoParaEspelho = false;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaInteracao))
        {
            olhandoParaEspelho = hit.collider.CompareTag("Mirror") ||
                                 (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Mirror")) ||
                                 hit.collider.CompareTag("Button");
        }

        textoAviso.SetActive(olhandoParaEspelho);
    }
}