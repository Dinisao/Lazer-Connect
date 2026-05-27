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
    }

    void Update()
    {
        if (jaAcabou) return;
        tempoAtual -= Time.deltaTime;
        if (tempoAtual <= 0) { tempoAtual = 0; StartCoroutine(SequenciaGameOver()); }
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

        // --- A PARTE QUE FALTA: DESLIGAR O LASER ---
        // Procura o script ControloLaser na cena e desliga-o
        ControloLaser scriptLaser = Object.FindFirstObjectByType<ControloLaser>();
        if (scriptLaser != null)
        {
            scriptLaser.laserAtivo = false;

            // Desliga imediatamente o som contínuo em loop do laser
            scriptLaser.PararSomLaser();
        }

        // 1. EXPLOSÃO
        Rigidbody[] todos = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in todos)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(forcaExplosao, transform.position, raioExplosao, 10f, ForceMode.VelocityChange);
        }

        // 2. TREMOR E FADE
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("PlayerCapsule");

        float tempoPassado = 0f;
        Vector3 posOriginal = (player != null) ? player.transform.position : Vector3.zero;

        while (tempoPassado < duracaoSequencia)
        {
            tempoPassado += Time.deltaTime;
            float progresso = tempoPassado / duracaoSequencia;

            if (player != null)
            {
                Vector3 tremor = Random.insideUnitSphere * intensidadeTremor * (1 - progresso);
                player.transform.position = posOriginal + tremor;
            }

            if (fadeImage != null)
            {
                fadeImage.color = new Color(0, 0, 0, progresso);
            }

            yield return null;
        }

        // 3. MENU PRINCIPAL
        // ADICIONADO: Liberta o rato e torna-o visível no menu principal
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(0);
    }
}