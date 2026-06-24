using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource audioSource;

    // ESCREVE AQUI OS NOMES EXATOS DAS TUAS CENAS DE MENU
    [SerializeField] private string nomeMainMenu = "Menu";
    [SerializeField] private string nomeMenuLevels = "MenuLevels";

    void Awake()
    {
        // Sistema Singleton para não duplicar a música
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // Diz ao Unity para avisar este script sempre que uma cena mudar
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Esta função corre automaticamente sempre que uma nova cena carrega
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se a nova cena for o Main Menu ou o Menu de Levels, toca a música
        if (scene.name == nomeMainMenu || scene.name == nomeMenuLevels)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // Se for qualquer outra cena (os níveis do jogo), para a música!
            audioSource.Stop();
        }
    }
}