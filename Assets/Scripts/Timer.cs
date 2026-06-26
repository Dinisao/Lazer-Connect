using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using FMODUnity; // IMPORTANTE: Adicionado para o Unity reconhecer o FMOD

public class TimerNivel : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    public float tempoInicial = 60f;
    public TextMeshProUGUI textoTimer;

    [Header("Referências UI")]
    public Image fadeImage;

    [Header("Sons do FMOD")]
    // Campo criado para selecionares o áudio da explosão no Inspector
    public EventReference somExplosao;

    [Header("Efeito de Caos")]
    public float forcaExplosao = 250f;
    public float raioExplosao = 60f;
    public float intensidadeTremor = 0.8f;
    public float duracaoSequencia = 3.0f;

    private float tempoAtual;
    private bool jaAcabou = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) { Destroy(gameObject); return; }
        tempoAtual = tempoInicial;
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);

        // MODIFICAÇÃO: Garante que o texto do timer nasce ESCONDIDO no início do jogo
        if (textoTimer != null)
        {
            textoTimer.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Se o laser ainda não foi ligado, o tempo não conta e o texto continua escondido!
        if (!ControloLaser.primeiroDisparoFeito) return;

        // MODIFICAÇÃO: No frame em que o laser liga, esta linha ativa o texto no ecrã!
        if (textoTimer != null && !textoTimer.gameObject.activeSelf)
        {
            textoTimer.gameObject.SetActive(true);
        }

        if (jaAcabou) return;

        tempoAtual -= Time.deltaTime;

        if (tempoAtual <= 0)
        {
            tempoAtual = 0;
            StartCoroutine(SequenciaGameOver());
        }

        AtualizarDisplay();
    }

    void AtualizarDisplay()
    {
        if (textoTimer == null) return;
        int minutos = Mathf.FloorToInt(tempoAtual / 60);
        int segundos = Mathf.FloorToInt(tempoAtual % 60);
        textoTimer.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        if (tempoAtual < 10f) textoTimer.color = Color.red;
    }

    IEnumerator SequenciaGameOver()
    {
        jaAcabou = true;

        // DISPARA O SOM DA EXPLOSÃO NO FMOD EXATAMENTE NO FRAME ZERO
        if (!somExplosao.IsNull)
        {
            RuntimeManager.PlayOneShot(somExplosao, transform.position);
        }

        // Procura o script ControloLaser na cena e desliga-o
        ControloLaser scriptLaser = Object.FindFirstObjectByType<ControloLaser>();
        if (scriptLaser != null)
        {
            scriptLaser.laserAtivo = false;
            scriptLaser.PararSomLaser();
        }

        // 1. EXPLOSÃO
        Rigidbody[] todos = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in todos)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(forcaExplosao, transform.position, raioExplosao, 10f, ForceMode.VelocityChange);
        }

        // 2. TREMOR E FADE (Acelerado para 1 segundo no total)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("PlayerCapsule");

        float tempoPassado = 0f;
        // Reduzimos à força o tempo de espera para 1 segundo (ignora o valor do inspector se quiseres)
        float tempoAlvo = 1.5f;
        Vector3 posOriginal = (player != null) ? player.transform.position : Vector3.zero;

        while (tempoPassado < tempoAlvo)
        {
            tempoPassado += Time.deltaTime;
            float progresso = tempoPassado / tempoAlvo;

            if (player != null)
            {
                Vector3 tremor = Random.insideUnitSphere * intensidadeTremor * (1 - progresso);
                player.transform.position = posOriginal + tremor;
            }

            if (fadeImage != null)
            {
                // O ecrã vai escurecer num piscar de olhos (1 segundo)
                fadeImage.color = new Color(0, 0, 0, progresso);
            }

            yield return null;
        }

        // Garantir que fica totalmente preto no fim do loop
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 1f);

        // 3. GAME OVER IMEDIATO
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Object.FindFirstObjectByType<MenuPausa>()?.MostrarGameOver();
    }
}