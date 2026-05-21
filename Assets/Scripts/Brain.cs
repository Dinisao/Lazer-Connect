using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using FMODUnity; // IMPORTANTE: Adicionado para o Unity reconhecer o FMOD

public class InteracaoFinal : MonoBehaviour
{
    [Header("Geral")]
    public float distanciaInteracao = 5f;
    public Transform pontoSegurar;
    public GameObject textoAviso;
    public GameObject objetoMira;

    [Header("Sons do Puzzle (FMOD)")]
    // Campo criado para selecionares o áudio do encaixe no Inspector
    public EventReference somColarEspelho;

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

    private GameObject objetoNaMao;
    private Rigidbody rbNaMao;
    private enum Tipo { Nada, Espelho, Caixa, Armario }
    private Tipo tipoAtual = Tipo.Nada;

    private Vector3 escalaOriginal;

    // Variáveis específicas para o armário
    private Vector3 distanciaInicialArmario;
    private Vector3 eixoMovimentoArmario;

    void Update()
    {
        if (objetoNaMao == null) VerificarMira();
        else ChequearDistanciaLimite();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (objetoNaMao == null) TentarInteragir();
            else LargarOuColar();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) TentarRodar();
    }

    void FixedUpdate()
    {
        if (objetoNaMao == null || rbNaMao == null) return;

        // Movimento suave do espelho na mão
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
        // Movimento restrito do armário
        else if (tipoAtual == Tipo.Armario)
        {
            Vector3 posicaoAlvo = pontoSegurar.position + distanciaInicialArmario;
            Vector3 direcaoLivre = posicaoAlvo - rbNaMao.position;
            direcaoLivre.y = 0;

            Vector3 direcaoRestrita = Vector3.Project(direcaoLivre, eixoMovimentoArmario);
            rbNaMao.linearVelocity = direcaoRestrita * forcaArrastarArmario;
        }
    }

    void ChequearDistanciaLimite()
    {
        if (objetoNaMao == null || rbNaMao == null) return;

        float distanciaAtual = Vector3.Distance(pontoSegurar.position, rbNaMao.position);

        if (distanciaAtual > distanciaInteracao + 1.5f)
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
            // CORREÇÃO CIRÚRGICA: Suporta tanto o botão antigo como o novo InterruptorLaser
            if (hit.collider.CompareTag("Button"))
            {
                InterruptorLaser botao = hit.collider.GetComponent<InterruptorLaser>();
                if (botao != null)
                {
                    botao.InteragirComInterruptor();
                }
                else
                {
                    Object.FindFirstObjectByType<ControloLaser>()?.AlternarLaser();
                }
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
                objetoNaMao.transform.localScale = escalaOriginal;
                objetoNaMao.transform.localPosition = offsetMaoCaixa;
                objetoNaMao.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            else if (hit.collider.CompareTag("Armario"))
            {
                rbNaMao = rb;
                objetoNaMao = rb.gameObject;
                tipoAtual = Tipo.Armario;
                AtualizarEstadoMira(true);

                rbNaMao.linearDamping = 5f;
                distanciaInicialArmario = rbNaMao.position - pontoSegurar.position;
                distanciaInicialArmario.y = 0;

                eixoMovimentoArmario = hit.normal;
                eixoMovimentoArmario.y = 0;
                eixoMovimentoArmario.Normalize();
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
        if (tipoAtual == Tipo.Espelho)
        {
            RaycastHit hit;
            Ray raio = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            if (Physics.Raycast(raio, out hit, distanciaColagem))
            {
                if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Chao"))
                {
                    // Cache das variáveis antes de limpar o estado principal
                    GameObject espelhoParaColar = objetoNaMao;
                    Rigidbody rbParaColar = rbNaMao;

                    // Desvincula imediatamente o objeto antes da Coroutine começar
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
            rbNaMao.linearDamping = 100f;
            rbNaMao.linearVelocity = Vector3.zero;
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

        // Tranca a física de forma absoluta no frame 1
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

        // TOCA O SOM DO FMOD EXATAMENTE NO MOMENTO DO SNAP (Antes de congelar na parede)
        if (!somColarEspelho.IsNull)
        {
            RuntimeManager.PlayOneShot(somColarEspelho, posFinal);
        }

        // Sistema multi-frame ultra agressivo para garantir que a transformação é injetada
        for (int i = 0; i < 3; i++)
        {
            espelho.transform.position = posFinal;
            espelho.transform.rotation = rotFinal;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            yield return new WaitForEndOfFrame();
        }

        // Reativa os colisores com segurança
        foreach (var c in espelho.GetComponentsInChildren<Collider>()) c.enabled = true;
    }

    void SoltarNoChao()
    {
        if (objetoNaMao == null) return;

        objetoNaMao.transform.SetParent(null);
        objetoNaMao.transform.localScale = escalaOriginal;
        tipoAtual = Tipo.Nada;

        Vector3 origem = Camera.main.transform.position;
        Vector3 direcao = objetoNaMao.transform.position - origem;

        if (Physics.Raycast(origem, direcao.normalized, out RaycastHit hit, direcao.magnitude))
        {
            objetoNaMao.transform.position = hit.point - (direcao.normalized * 0.2f);
        }

        rbNaMao.isKinematic = false;
        rbNaMao.useGravity = true;
        foreach (var c in objetoNaMao.GetComponentsInChildren<Collider>()) c.enabled = true;

        objetoNaMao = null; rbNaMao = null;
    }

    void TentarRodar()
    {
        if (objetoNaMao != null && tipoAtual == Tipo.Espelho)
        {
            objetoNaMao.GetComponent<MirrorRotate>()?.Rotate();
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