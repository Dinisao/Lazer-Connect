using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // FUNDAMENTAL para controlar a lente do desfoque
using System.Collections;

public class PiscarAcordar : MonoBehaviour
{
    [Header("As Pálpebras (UI Images)")]
    public RectTransform palpebraCima;
    public RectTransform palpebraBaixo;

    [Header("O Desfoque da Visão (Global Volume)")]
    public Volume volumeDesfoque;

    [Header("Tempos (segundos)")]
    public float tempoTotalDesfoque = 10.0f; // 10 segundos para a visão ficar 100% limpa

    private GameObject player;
    private Vector3 posicaoFixa;
    private bool estaParalisado = false;
    private float alturaMaxima; // Calculada automaticamente para o teu ecrã

    private DepthOfField dof; // O controlo direto da lente!

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            posicaoFixa = player.transform.position;
            estaParalisado = true;
        }

        alturaMaxima = (Screen.height / 2f) + 50f;

        if (palpebraCima != null && palpebraBaixo != null)
        {
            palpebraCima.anchorMin = new Vector2(0, 1);
            palpebraCima.anchorMax = new Vector2(1, 1);
            palpebraCima.pivot = new Vector2(0.5f, 1);
            palpebraCima.anchoredPosition = Vector2.zero;

            palpebraBaixo.anchorMin = new Vector2(0, 0);
            palpebraBaixo.anchorMax = new Vector2(1, 0);
            palpebraBaixo.pivot = new Vector2(0.5f, 0);
            palpebraBaixo.anchoredPosition = Vector2.zero;

            ForcarAbertura(0f);
            StartCoroutine(SequenciaDramaticaAAA());
        }

        // AQUI ESTÁ A MAGIA NOVA:
        if (volumeDesfoque != null)
        {
            // Cria um clone temporário para não estragar o ficheiro original do Unity
            if (volumeDesfoque.HasInstantiatedProfile() == false)
            {
                volumeDesfoque.profile = Instantiate(volumeDesfoque.sharedProfile);
            }

            if (volumeDesfoque.profile.TryGet<DepthOfField>(out dof))
            {
                dof.active = true;
                dof.mode.overrideState = true;
                dof.mode.value = DepthOfFieldMode.Bokeh;

                dof.focusDistance.overrideState = true;
                dof.focusDistance.value = 0.1f; // Força turvo total

                dof.focalLength.overrideState = true;
                dof.focalLength.value = 300f; // Força lente desfocada

                volumeDesfoque.weight = 1f;
                StartCoroutine(TirarDesfoqueDaLente());
            }
        }
    }

    void LateUpdate()
    {
        if (estaParalisado && player != null)
        {
            player.transform.position = posicaoFixa;
        }
    }

    IEnumerator SequenciaDramaticaAAA()
    {
        // 1. Escuridão total no início
        yield return new WaitForSeconds(1.5f);

        // --- PISCADELA 1 (A Preguiçosa) ---
        // Abre só 15% (quase nada) a tremer e volta a fechar
        yield return StartCoroutine(MoverPalpebras(0f, 0.15f, 1.5f));
        yield return new WaitForSeconds(0.2f); // Aguenta a nesga aberta
        yield return StartCoroutine(MoverPalpebras(0.15f, 0f, 0.8f)); // Fecha pesado

        yield return new WaitForSeconds(0.8f);

        // --- PISCADELA 2 (A Tentativa e Falha) ---
        // Abre até meio (50%) com mais força
        yield return StartCoroutine(MoverPalpebras(0f, 0.5f, 1.0f));
        yield return new WaitForSeconds(0.1f); // Vacila

        // FECHO BRUTO E INSTANTÂNEO (Músculo cedeu)
        ForcarAbertura(0f);

        yield return new WaitForSeconds(0.4f);

        // --- ACORDAR FINAL (A Puxada) ---
        // Abre 100% (começa rápido e suaviza no fim)
        yield return StartCoroutine(MoverPalpebras(0f, 1.0f, 2.0f, true));

        estaParalisado = false;
        palpebraCima.gameObject.SetActive(false);
        palpebraBaixo.gameObject.SetActive(false);
    }

    // Altera apenas a Altura (Height) das imagens
    void ForcarAbertura(float percentagemAberto)
    {
        // Se percentagem = 0, a altura é máxima (tapa tudo). Se for 1, a altura é 0 (vês o jogo).
        float alturaAtual = Mathf.Lerp(alturaMaxima, 0f, percentagemAberto);
        palpebraCima.sizeDelta = new Vector2(0, alturaAtual);
        palpebraBaixo.sizeDelta = new Vector2(0, alturaAtual);
    }

    IEnumerator MoverPalpebras(float inicio, float fim, float tempoTotal, bool rapidoNoInicio = false)
    {
        float tempo = 0;
        while (tempo < tempoTotal)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / tempoTotal;

            float t;
            if (rapidoNoInicio)
            {
                t = Mathf.Sin(progresso * Mathf.PI * 0.5f); // Ease-out (rápido no início, trava no fim)
            }
            else
            {
                t = Mathf.SmoothStep(0f, 1f, progresso); // Suave
            }

            ForcarAbertura(Mathf.Lerp(inicio, fim, t));
            yield return null;
        }
        ForcarAbertura(fim);
    }

    // COROUTINE QUE LIMPA A LENTE GRADUALMENTE
    IEnumerator TirarDesfoqueDaLente()
    {
        if (dof == null) yield break;

        float tempo = 0;
        while (tempo < tempoTotalDesfoque)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / tempoTotalDesfoque;

            // Transforma a lente gigante turva (300) numa lente normal limpa (ex: 30)
            dof.focalLength.value = Mathf.Lerp(300f, 30f, progresso);

            // Empurra a distância de foco de 10cm (0.1) para o infinito (ex: 15 metros)
            dof.focusDistance.value = Mathf.Lerp(0.1f, 15f, progresso);

            yield return null;
        }
    }
}