using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem;

public class FMODMenuMusicManager : MonoBehaviour
{
    private static FMODMenuMusicManager instance;

    [Header("Configuração FMOD")]
    // Arrasta para aqui o evento da tua música do FMOD
    [SerializeField] private EventReference musicaMenuEvent;

    private EventInstance instanceMusica;

    [Header("Nomes das Cenas")]
    [SerializeField] private string nomeMainMenu = "MainMenu";
    [SerializeField] private string nomeMenuLevels = "MenuLevels";

    void Awake()
    {
        // Sistema Singleton para garantir que só existe um gestor de música
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Criar a instância do evento do FMOD
        if (!musicaMenuEvent.IsNull)
        {
            instanceMusica = RuntimeManager.CreateInstance(musicaMenuEvent);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se entrou no Main Menu ou no Menu de Levels
        if (scene.name == nomeMainMenu || scene.name == nomeMenuLevels)
        {
            PLAYBACK_STATE state;
            instanceMusica.getPlaybackState(out state);

            // Se a música não estiver a tocar, inicia-a
            if (state != PLAYBACK_STATE.PLAYING)
            {
                instanceMusica.start();
            }
        }
        else
        {
            // Se entrou num nível real de jogo, para a música dos menus!
            // STOP_MODE.ALLOWFADEOUT respeita o fade out que configuraste no FMOD Studio
            instanceMusica.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    void OnDestroy()
    {
        // Boa prática: libertar a memória do FMOD quando o jogo fecha
        instanceMusica.release();
    }
}