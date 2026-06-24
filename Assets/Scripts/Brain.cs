using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using FMODUnity;

public class InteracaoFinal : MonoBehaviour
{
    [Header("Geral")]
    public float distanciaInteracao = 5f;
    public Transform pontoSegurar;
    public GameObject textoAviso;
    public GameObject objetoMira;

    [Header("Sons do Puzzle (FMOD)")]
    public EventReference somColarEspelho;
    public EventReference somRodarEspelho; // ADICIONADO: O teu novo som de rotação!

    [Header("Ajuste do ESPELHO")]
    public float distanciaColagem = 4f;
    public float offsetColagemMirror = 0.05f;
    public Vector3 offsetMaoMirror = new Vector3(0, 0, 0);
    public float forcaSeguirMirror = 25f;

    [Header("Sistema de Grelha")]
    public float tamanhoGrelha = 1f;
    public Vector3 offsetGrelha = new Vector3(0, 0, 0);

    [Header("Ajuste da CAIXA")]
    public Vector3 offsetMaoCaixa = new Vector3(0, -0.2f, 0.5f);

    [Header("Ajuste do ARMARIO")]
    public float forcaArrastarArmario = 15f;

    public static bool segurandoArmario = false;

    private GameObject objetoNaMao;
    private Rigidbody rbNaMao;
    private enum Tipo { Nada, Espelho, Caixa, Armario }
    private Tipo tipoAtual = Tipo.Nada;

    private Vector3 escalaOriginal;
    private Vector3 distanciaInicialArmario;
    private Vector3 eixoMovimentoArmario;
    private float distanciaOriginalAoAgarrar;

    private Collider colisorJogador;
    private Collider colisorArmarioNaMao;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        colisorJogador = GetComponent<Collider>();
        if (colisorJogador == null) colisorJogador = GetComponentInChildren<Collider>();
        if (colisorJogador == null) colisorJogador = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (MenuPausa.jogoPausado) return;

        if (objetoNaMao == null) VerificarMira();
        else ChequearDistanciaLimite();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (objetoNaMao == null) TentarInteragir();
            else LargarOuColar();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) TentarRodar();

        // FORÇA BRUTA PARA A CAIXA: Mantém a caixa estática no sítio certo sem stress
        if (objetoNaMao != null && tipoAtual == Tipo.Caixa)
        {
            objetoNaMao.transform.localPosition = offsetMaoCaixa;
        }
    }

    void FixedUpdate()
    {
        if (objetoNaMao == null || rbNaMao == null) return;

        if (tipoAtual == Tipo.Espelho)
        {
            Vector3 posAlvo = pontoSegurar.position +
                             pontoSegurar.right * offsetMaoMirror.x +
                             pontoSegurar.up * offsetMaoMirror.y +
                             pontoSegurar.forward * offsetMaoMirror.z;

            rbNaMao.MovePosition(Vector3.Lerp(rbNaMao.position, posAlvo, Time.fixedDeltaTime * forcaSeguirMirror));

            Quaternion rotAlvo = pontoSegurar.rotation * Quaternion.Euler(0, 180, 0);
            rbNaMao.MoveRotation(Quaternion.Slerp(rbNaMao.rotation, rotAlvo, Time.fixedDeltaTime * forcaSeguirMirror));
        }
        else if (tipoAtual == Tipo.Armario)
        {
            Vector3 dirPlana = Camera.main.transform.forward;
            dirPlana.y = 0;
            dirPlana.Normalize();

            Vector3 pontoVirtual = Camera.main.transform.position + (dirPlana * distanciaInteracao);
            Vector3 posicaoAlvo = pontoVirtual + distanciaInicialArmario;

            Vector3 direcaoLivre = posicaoAlvo - rbNaMao.position;
            direcaoLivre.y = 0;

            Vector3 direcaoRestrita = Vector3.Project(direcaoLivre, eixoMovimentoArmario);
            rbNaMao.linearVelocity = direcaoRestrita * forcaArrastarArmario;
        }
    }

    void ChequearDistanciaLimite()
    {
        if (objetoNaMao == null || rbNaMao == null || tipoAtual != Tipo.Armario) return;

        float distAtual = Vector3.Distance(Camera.main.transform.position, rbNaMao.position);

        if (distAtual > distanciaOriginalAoAgarrar + 1.5f || distAtual < distanciaOriginalAoAgarrar - 0.7f)
        {
            LargarOuColar();
        }
    }

    void TentarInteragir()
    {
        RaycastHit hit;
        Ray raio = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(raio, out hit, distanciaInteracao))
        {
            if (hit.collider.CompareTag("Button"))
            {
                hit.collider.GetComponent<StudioEventEmitter>()?.Play();
                InterruptorLaser botao = hit.collider.GetComponent<InterruptorLaser>();
                if (botao != null) botao.InteragirComInterruptor();
                else Object.FindFirstObjectByType<ControloLaser>()?.AlternarLaser();
                return;
            }

            if (hit.collider.CompareTag("FixedMirror")) return;

            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();
            if (rb == null) return;

            if (hit.collider.CompareTag("Mirror"))
            {
                ConfigurarPegar(rb, Tipo.Espelho);
                AtualizarEstadoMira(false);
            }
            else if (hit.collider.CompareTag("Caixa"))
            {
                ConfigurarPegar(rb, Tipo.Caixa);
                AtualizarEstadoMira(false);

                objetoNaMao.transform.SetParent(pontoSegurar);
                objetoNaMao.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            else if (hit.collider.CompareTag("Armario"))
            {
                rbNaMao = rb;
                objetoNaMao = rb.gameObject;
                tipoAtual = Tipo.Armario;
                segurandoArmario = true;
                AtualizarEstadoMira(true);

                colisorArmarioNaMao = rb.GetComponent<Collider>();
                if (colisorArmarioNaMao == null) colisorArmarioNaMao = rb.GetComponentInChildren<Collider>();

                if (colisorJogador != null && colisorArmarioNaMao != null)
                {
                    Physics.IgnoreCollision(colisorJogador, colisorArmarioNaMao, true);
                }

                distanciaOriginalAoAgarrar = Vector3.Distance(Camera.main.transform.position, rbNaMao.position);

                Vector3 dirPlana = Camera.main.transform.forward;
                dirPlana.y = 0;
                dirPlana.Normalize();
                Vector3 pontoVirtual = Camera.main.transform.position + (dirPlana * distanciaInteracao);

                distanciaInicialArmario = rbNaMao.position - pontoVirtual;
                distanciaInicialArmario.y = 0;

                eixoMovimentoArmario = hit.normal;
                eixoMovimentoArmario.y = 0;
                eixoMovimentoArmario.Normalize();

                rbNaMao.isKinematic = false;
                rbNaMao.useGravity = true;
                rbNaMao.linearDamping = 5f;
                rbNaMao.angularDamping = 10f;
            }
        }
    }

    void ConfigurarPegar(Rigidbody rb, Tipo t)
    {
        rbNaMao = rb;
        objetoNaMao = rb.gameObject;
        tipoAtual = t;
        escalaOriginal = objetoNaMao.transform.localScale;

        rbNaMao.isKinematic = true;
        rbNaMao.useGravity = false;
        foreach (var c in objetoNaMao.GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    void LargarOuColar()
    {
        if (tipoAtual == Tipo.Armario && colisorJogador != null && colisorArmarioNaMao != null)
        {
            Physics.IgnoreCollision(colisorJogador, colisorArmarioNaMao, false);
            colisorArmarioNaMao = null;
        }

        if (tipoAtual == Tipo.Espelho)
        {
            RaycastHit hit;
            Ray raio = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            if (Physics.Raycast(raio, out hit, distanciaColagem))
            {
                if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Chao"))
                {
                    GameObject espelhoParaColar = objetoNaMao;
                    Rigidbody rbParaColar = rbNaMao;

                    espelhoParaColar.transform.SetParent(null);
                    objetoNaMao = null; rbNaMao = null; tipoAtual = Tipo.Nada;
                    AtualizarEstadoMira(true);

                    StartCoroutine(ColarParede(hit.point, hit.normal, espelhoParaColar, rbParaColar));
                    return;
                }
            }
            SoltarNoChao();
            AtualizarEstadoMira(true);
        }
        else if (tipoAtual == Tipo.Armario)
        {
            rbNaMao.linearVelocity = Vector3.zero;
            rbNaMao.angularVelocity = Vector3.zero;
            rbNaMao.linearDamping = 0.5f;

            segurandoArmario = false;
            objetoNaMao = null; rbNaMao = null; tipoAtual = Tipo.Nada;
            AtualizarEstadoMira(true);
        }
        else
        {
            SoltarNoChao();
            AtualizarEstadoMira(true);
        }
    }

    IEnumerator ColarParede(Vector3 ponto, Vector3 normal, GameObject espelho, Rigidbody rb)
    {
        if (espelho == null || rb == null) yield break;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 snapPos = ponto;
        float Round(float v, float t, float o) => t == 0 ? v : Mathf.Round((v - o) / t) * t + o;

        if (Mathf.Abs(normal.x) < 0.5f) snapPos.x = Round(snapPos.x, tamanhoGrelha, offsetGrelha.x);
        if (Mathf.Abs(normal.y) < 0.5f) snapPos.y = Round(snapPos.y, tamanhoGrelha, offsetGrelha.y);
        if (Mathf.Abs(normal.z) < 0.5f) snapPos.z = Round(snapPos.z, tamanhoGrelha, offsetGrelha.z);

        Vector3 posFinal = snapPos + (normal * offsetColagemMirror);
        Quaternion rotFinal;

        if (Mathf.Abs(normal.y) > 0.8f)
        {
            Vector3 direcaoSnap = Mathf.Abs(transform.forward.x) > Mathf.Abs(transform.forward.z) ?
                new Vector3(Mathf.Sign(transform.forward.x), 0, 0) : new Vector3(0, 0, Mathf.Sign(transform.forward.z));
            rotFinal = Quaternion.LookRotation(normal, direcaoSnap);
        }
        else
        {
            rotFinal = Quaternion.LookRotation(normal, Vector3.up);
        }

        if (!somColarEspelho.IsNull)
        {
            RuntimeManager.PlayOneShot(somColarEspelho, posFinal);
        }

        for (int i = 0; i < 3; i++)
        {
            espelho.transform.position = posFinal;
            espelho.transform.rotation = rotFinal;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            yield return new WaitForEndOfFrame();
        }

        foreach (var c in espelho.GetComponentsInChildren<Collider>()) c.enabled = true;
    }

    void SoltarNoChao()
    {
        if (objetoNaMao == null) return;

        objetoNaMao.transform.SetParent(null);
        objetoNaMao.transform.localScale = escalaOriginal;

        // O teu sistema da bolha gorda perfeito que impede de atravessar colunas
        Vector3 origem = Camera.main.transform.position;
        Vector3 direcao = objetoNaMao.transform.position - origem;

        if (Physics.SphereCast(origem, 0.25f, direcao.normalized, out RaycastHit hit, direcao.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            objetoNaMao.transform.position = hit.point + (hit.normal * 0.35f);
        }

        rbNaMao.isKinematic = false;
        rbNaMao.useGravity = true;
        rbNaMao.linearVelocity = Vector3.zero;
        rbNaMao.angularVelocity = Vector3.zero;

        foreach (var c in objetoNaMao.GetComponentsInChildren<Collider>()) c.enabled = true;

        tipoAtual = Tipo.Nada;
        objetoNaMao = null;
        rbNaMao = null;
    }

    void TentarRodar()
    {
        if (objetoNaMao != null && tipoAtual == Tipo.Espelho)
        {
            objetoNaMao.GetComponent<MirrorRotate>()?.Rotate();

            // TOCA O SOM QUANDO RODAS O ESPELHO QUE TENS NA MÃO
            if (!somRodarEspelho.IsNull)
            {
                RuntimeManager.PlayOneShot(somRodarEspelho, objetoNaMao.transform.position);
            }
        }
        else if (objetoNaMao == null)
        {
            RaycastHit hit;
            Ray raio = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(raio, out hit, distanciaInteracao))
            {
                if (hit.collider.CompareTag("FixedMirror") || hit.collider.CompareTag("Mirror"))
                {
                    hit.collider.GetComponentInParent<MirrorRotate>()?.Rotate();

                    // TOCA O SOM QUANDO RODAS UM ESPELHO À DISTÂNCIA (NA PAREDE)
                    if (!somRodarEspelho.IsNull)
                    {
                        RuntimeManager.PlayOneShot(somRodarEspelho, hit.point);
                    }
                }
            }
        }
    }

    void VerificarMira()
    {
        if (textoAviso == null) return;
        RaycastHit hit;
        Ray raio = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        bool ok = false;
        if (Physics.Raycast(raio, out hit, distanciaInteracao))
        {
            ok = hit.collider.CompareTag("Mirror") || hit.collider.CompareTag("Caixa") ||
                 hit.collider.CompareTag("FixedMirror") || hit.collider.CompareTag("Button") ||
                 hit.collider.CompareTag("Armario");
        }
        textoAviso.SetActive(ok);
    }

    void AtualizarEstadoMira(bool ligada)
    {
        if (objetoMira != null)
        {
            objetoMira.SetActive(ligada);
        }
    }
}
