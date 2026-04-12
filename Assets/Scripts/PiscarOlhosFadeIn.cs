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
    public float tempoTotalDesfoque = 4.0f;

    private GameObject player;
    private Vector3 posicaoFixa;
    private bool estaParalisado = false;
    private float alturaMaxima;

    private DepthOfField dof;

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
    }

    void Start()
    {
        if (palpebraCima != null && palpebraBaixo != null)
        {
            StartCoroutine(SequenciaDesorientada());
        }

        if (volumeDesfoque != null && volumeDesfoque.profile.TryGet<DepthOfField>(out dof))
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

        // --- PISCADELA 1: Humana e rápida ---
        // Abre um bocadinho (0.15s) e fecha logo (0.1s)
        yield return StartCoroutine(MoverPalpebras(0f, 0.3f, 0.15f / multiplicadorVelocidade));
        yield return StartCoroutine(MoverPalpebras(0.3f, 0f, 0.1f / multiplicadorVelocidade));

        yield return new WaitForSeconds(0.4f);

        // --- PISCADELA 2: Vacila mas não desiste ---
        // Abre até mais de meio, quase fecha, e volta a abrir
        yield return StartCoroutine(MoverPalpebras(0f, 0.6f, 0.15f / multiplicadorVelocidade));
        yield return StartCoroutine(MoverPalpebras(0.6f, 0.2f, 0.1f / multiplicadorVelocidade));
        yield return StartCoroutine(MoverPalpebras(0.2f, 0.7f, 0.15f / multiplicadorVelocidade));
        yield return StartCoroutine(MoverPalpebras(0.7f, 0f, 0.1f / multiplicadorVelocidade));

        yield return new WaitForSeconds(0.3f);

        // --- ABERTURA FINAL: Rápida mas suave no fim ---
        // Demora apenas meio segundo (0.6s) a abrir de vez. Um piscar de olhos normal!
        yield return StartCoroutine(MoverPalpebras(0f, 1.0f, 0.6f / multiplicadorVelocidade, true));

        estaParalisado = false;
        palpebraCima.gameObject.SetActive(false);
        palpebraBaixo.gameObject.SetActive(false);
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
                // Ease-Out: Arranca rápido (como um músculo a puxar) e trava suavemente no fim
                t = Mathf.Sin(progresso * Mathf.PI * 0.5f);
            }
            else
            {
                // SmoothStep: Curva orgânica humana para piscadelas normais (acelera e desacelera)
                t = Mathf.SmoothStep(0f, 1f, progresso);
            }

            ForcarAbertura(Mathf.Lerp(inicio, fim, t));
            yield return null;
        }
        ForcarAbertura(fim);
    }

    IEnumerator TirarDesfoqueDaLente()
    {
        float tempo = 0;
        while (tempo < tempoTotalDesfoque)
        {
            tempo += Time.deltaTime;
            float p = tempo / tempoTotalDesfoque;
            if (dof != null)
            {
                dof.focalLength.value = Mathf.Lerp(300f, 30f, p);
                dof.focusDistance.value = Mathf.Lerp(0.1f, 10f, p);
                dof.aperture.value = Mathf.Lerp(1f, 16f, p);
            }
            yield return null;
        }
    }
}