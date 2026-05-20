using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PiscarAcordar : MonoBehaviour
{
    [Header("As Pálpebras (UI Images)")]
    public RectTransform palpebraCima;
    public RectTransform palpebraBaixo;

    [Header("O Desfoque da Visão (Global Volume)")]
    public Volume volumeDesfoque;

    [Header("Ajustes de Velocidade")]
    public float multiplicadorVelocidade = 1.0f;
    public float tempoTotalDesfoque = 7.0f;

    [Tooltip("Espera estes segundos antes de começar a focar a visão!")]
    public float atrasoParaFocar = 3.0f;

    private GameObject player;
    private Vector3 posicaoFixa;
    private bool estaParalisado = false;
    private float alturaMaxima;

    private DepthOfField dof;
    private ChromaticAberration chromatic;
    private LensDistortion distortion;
    private Vignette vignette; // Adicionado para controlar o escurecimento das bordas

    // Variáveis adicionadas para gerir a mira no início
    private InteracaoFinal scriptInteracao;
    private GameObject crosshairDoJogador;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            posicaoFixa = player.transform.position;
            estaParalisado = true;
        }

        alturaMaxima = Screen.height;

        if (palpebraCima != null && palpebraBaixo != null)
        {
            ConfigurarAncoras();
            ForcarAbertura(0f);
        }

        // --- OCULTAR A MIRA NO RESPRAWN/INÍCIO ---
        scriptInteracao = Object.FindFirstObjectByType<InteracaoFinal>();
        if (scriptInteracao != null)
        {
            if (scriptInteracao.textoAviso != null)
                scriptInteracao.textoAviso.SetActive(false);

            if (scriptInteracao.objetoMira != null)
            {
                crosshairDoJogador = scriptInteracao.objetoMira;
                crosshairDoJogador.SetActive(false);
            }
        }
    }

    void Start()
    {
        if (palpebraCima != null && palpebraBaixo != null)
        {
            StartCoroutine(SequenciaDesorientada());
        }

        if (volumeDesfoque != null)
        {
            // Configura o Desfoque de Lente
            if (volumeDesfoque.profile.TryGet<DepthOfField>(out dof))
            {
                dof.active = true;
                dof.mode.overrideState = true;
                dof.mode.value = DepthOfFieldMode.Bokeh;
                dof.focusDistance.overrideState = true;
                dof.focusDistance.value = 0.1f;
                dof.focalLength.overrideState = true;
                dof.focalLength.value = 300f;
                dof.aperture.overrideState = true;
                dof.aperture.value = 1f;
            }

            // Configura a Visão Dupla/Tontura de Cores
            if (volumeDesfoque.profile.TryGet<ChromaticAberration>(out chromatic))
            {
                chromatic.active = true;
                chromatic.intensity.overrideState = true;
                chromatic.intensity.value = 1.0f;
            }

            // Configura a Distorção de Lente
            if (volumeDesfoque.profile.TryGet<LensDistortion>(out distortion))
            {
                distortion.active = true;
                distortion.intensity.overrideState = true;
                distortion.intensity.value = -12f;
            }

            // NOVO: Configura as bordas pretas (Vignette) para começarem ativas no início
            if (volumeDesfoque.profile.TryGet<Vignette>(out vignette))
            {
                vignette.active = true;
                vignette.intensity.overrideState = true;
                vignette.intensity.value = 0.45f; // Força inicial do escurecimento
                vignette.smoothness.overrideState = true;
                vignette.smoothness.value = 1.0f;
            }

            StartCoroutine(TirarDesfoqueDaLente());
        }
    }

    void ConfigurarAncoras()
    {
        palpebraCima.anchorMin = new Vector2(0, 1);
        palpebraCima.anchorMax = new Vector2(1, 1);
        palpebraCima.pivot = new Vector2(0.5f, 1);
        palpebraBaixo.anchorMin = new Vector2(0, 0);
        palpebraBaixo.anchorMax = new Vector2(1, 0);
        palpebraBaixo.pivot = new Vector2(0.5f, 0);
    }

    void LateUpdate()
    {
        if (estaParalisado && player != null)
            player.transform.position = posicaoFixa;
    }

    IEnumerator SequenciaDesorientada()
    {
        yield return new WaitForSeconds(1.0f);

        // --- PISCADELA 1 ---
        yield return StartCoroutine(MoverPalpebras(0f, 0.3f, 0.15f / multiplicadorVelocidade));
        yield return StartCoroutine(MoverPalpebras(0.3f, 0f, 0.1f / multiplicadorVelocidade));

        yield return new WaitForSeconds(0.4f);

        // --- PISCADELA 2 ---
        yield return StartCoroutine(MoverPalpebras(0f, 0.6f, 0.15f / multiplicadorVelocidade));
        yield return StartCoroutine(MoverPalpebras(0.6f, 0.2f, 0.1f / multiplicadorVelocidade));
        yield return StartCoroutine(MoverPalpebras(0.2f, 0.7f, 0.15f / multiplicadorVelocidade));
        yield return StartCoroutine(MoverPalpebras(0.7f, 0f, 0.1f / multiplicadorVelocidade));

        yield return new WaitForSeconds(0.3f);

        // --- ABERTURA FINAL ---
        yield return StartCoroutine(MoverPalpebras(0f, 1.0f, 0.6f / multiplicadorVelocidade, true));

        estaParalisado = false;
        palpebraCima.gameObject.SetActive(false);
        palpebraBaixo.gameObject.SetActive(false);

        if (crosshairDoJogador != null)
        {
            crosshairDoJogador.SetActive(true);
        }
    }

    void ForcarAbertura(float percentagemAberto)
    {
        float alturaAtual = Mathf.Lerp(alturaMaxima, 0f, percentagemAberto);
        palpebraCima.sizeDelta = new Vector2(0, alturaAtual);
        palpebraBaixo.sizeDelta = new Vector2(0, alturaAtual);
    }

    IEnumerator MoverPalpebras(float inicio, float fim, float tempoTotal, bool abrirDeVez = false)
    {
        float tempo = 0;
        while (tempo < tempoTotal)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / tempoTotal;

            float t;
            if (abrirDeVez)
            {
                t = Mathf.Sin(progresso * Mathf.PI * 0.5f);
            }
            else
            {
                t = Mathf.SmoothStep(0f, 1f, progresso);
            }

            ForcarAbertura(Mathf.Lerp(inicio, fim, t));
            yield return null;
        }
        ForcarAbertura(fim);
    }

    IEnumerator TirarDesfoqueDaLente()
    {
        yield return new WaitForSeconds(atrasoParaFocar);

        float tempo = 0;
        while (tempo < tempoTotalDesfoque)
        {
            tempo += Time.deltaTime;
            float p = tempo / tempoTotalDesfoque;

            // Diminui o desfoque gradualmente
            if (dof != null)
            {
                dof.focalLength.value = Mathf.Lerp(300f, 30f, p);
                dof.focusDistance.value = Mathf.Lerp(0.1f, 10f, p);
                dof.aperture.value = Mathf.Lerp(1f, 16f, p);
            }

            // Diminui a aberração cromática até 0
            if (chromatic != null)
            {
                chromatic.intensity.value = Mathf.Lerp(1.0f, 0f, p);
            }

            // Diminui a distorção suavemente até 0
            if (distortion != null)
            {
                distortion.intensity.value = Mathf.Lerp(-12f, 0f, p);
            }

            // NOVO: Diminui a Vignette (bordas pretas) suavemente de 0.45f até 0f
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(0.45f, 0f, p);
            }

            yield return null;
        }

        // Desativa tudo por completo no fim para garantir ecrã 100% limpo
        if (dof != null) dof.active = false;
        if (chromatic != null) chromatic.active = false;
        if (distortion != null) distortion.active = false;
        if (vignette != null) vignette.active = false; // Desliga a Vignette completamente
    }
}