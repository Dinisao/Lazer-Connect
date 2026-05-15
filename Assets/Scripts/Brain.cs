using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InteracaoFinal : MonoBehaviour
{
    [Header("Geral")]
    public float distanciaInteracao = 5f;
    public Transform pontoSegurar;
    public GameObject textoAviso;
    public GameObject objetoMira; // Drag & Drop da tua Mira do Canvas para aqui

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
            if (hit.collider.CompareTag("Button"))
            {
                Object.FindFirstObjectByType<ControloLaser>()?.AlternarLaser();
                return;
            }

            if (hit.collider.CompareTag("FixedMirror")) return;

            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();
            if (rb == null) return;

            if (hit.collider.CompareTag("Mirror"))
            {
                ConfigurarPegar(rb, Tipo.Espelho);
                AtualizarEstadoMira(false); // Esconde a mira ao pegar no espelho
            }
            else if (hit.collider.CompareTag("Caixa"))
            {
                ConfigurarPegar(rb, Tipo.Caixa);
                AtualizarEstadoMira(false); // Esconde a mira ao pegar na caixa

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
                AtualizarEstadoMira(true); // Mantém a mira visível para o armário

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
                    StartCoroutine(ColarParede(hit.point, hit.normal));
                    return;
                }
            }
            SoltarNoChao();
            AtualizarEstadoMira(true); // Volta a mostrar a mira ao largar
        }
        else if (tipoAtual == Tipo.Armario)
        {
            rbNaMao.linearDamping = 100f;
            rbNaMao.linearVelocity = Vector3.zero;
            objetoNaMao = null; rbNaMao = null; tipoAtual = Tipo.Nada;
            AtualizarEstadoMira(true); // Garante que a mira está ligada
        }
        else
        {
            SoltarNoChao();
            AtualizarEstadoMira(true); // Volta a mostrar a mira ao largar a caixa
        }
    }

    IEnumerator ColarParede(Vector3 ponto, Vector3 normal)
    {
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

        for (int i = 0; i < 3; i++)
        {
            rbNaMao.isKinematic = false;
            rbNaMao.linearVelocity = Vector3.zero;
            rbNaMao.angularVelocity = Vector3.zero;
            rbNaMao.isKinematic = true;
            rbNaMao.useGravity = false;

            objetoNaMao.transform.position = posFinal;
            objetoNaMao.transform.rotation = rotFinal;

            yield return new WaitForEndOfFrame();
        }

        foreach (var c in objetoNaMao.GetComponentsInChildren<Collider>()) c.enabled = true;

        objetoNaMao = null; rbNaMao = null; tipoAtual = Tipo.Nada;
        AtualizarEstadoMira(true); // Mostra a mira depois do snap do espelho terminar
    }

    void SoltarNoChao()
    {
        if (objetoNaMao == null) return;

        objetoNaMao.transform.SetParent(null);
        objetoNaMao.transform.localScale = escalaOriginal;

        Vector3 origem = Camera.main.transform.position;
        Vector3 direcao = objetoNaMao.transform.position - origem;

        if (Physics.Raycast(origem, direcao.normalized, out RaycastHit hit, direcao.magnitude))
        {
            objetoNaMao.transform.position = hit.point - (direcao.normalized * 0.2f);
        }

        rbNaMao.isKinematic = false;
        rbNaMao.useGravity = true;
        foreach (var c in objetoNaMao.GetComponentsInChildren<Collider>()) c.enabled = true;

        objetoNaMao = null; rbNaMao = null; tipoAtual = Tipo.Nada;
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

    // Função auxiliar para ligar/desligar a mira de forma segura
    void AtualizarEstadoMira(bool ligada)
    {
        if (objetoMira != null)
        {
            objetoMira.SetActive(ligada);
        }
    }
}