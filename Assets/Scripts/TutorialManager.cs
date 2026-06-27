using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject painelTutorialCanvas;
    public GameObject videoQuad;

    [Header("Configuracoes")]
    public float tempoEsperaAntesDeMostrar = 5f;
    public float tempoDuracaoDoTutorial = 10f;

    private bool tutorialMostrado = false;

    void Start()
    {
        if (painelTutorialCanvas != null) painelTutorialCanvas.SetActive(false);
        if (videoQuad != null) videoQuad.SetActive(false);

        StartCoroutine(ContagemTutorial());
    }

    IEnumerator ContagemTutorial()
    {
        // 1. Espera os 5 segundos iniciais para mostrar
        yield return new WaitForSeconds(tempoEsperaAntesDeMostrar);

        DispararTutorial();

        // 2. Espera os 10 segundos com o tutorial no ecrã
        yield return new WaitForSecondsRealtime(tempoDuracaoDoTutorial); // Usa Realtime porque o Time.timeScale vai estar a 0

        FecharTutorialAutomatico();
    }

    void DispararTutorial()
    {
        if (tutorialMostrado || painelTutorialCanvas == null) return;

        tutorialMostrado = true;
        painelTutorialCanvas.SetActive(true);
        if (videoQuad != null) videoQuad.SetActive(true);

        // Congela o jogo
        Time.timeScale = 0f;
    }

    void FecharTutorialAutomatico()
    {
        // Limpa tudo do ecrã
        if (videoQuad != null) videoQuad.SetActive(false);
        if (painelTutorialCanvas != null) painelTutorialCanvas.SetActive(false);

        // Descongela o jogo e deixa o player jogar normal
        Time.timeScale = 1f;

        // Desativa o manager para acabar a execução
        this.enabled = false;
        Destroy(gameObject);
    }
}