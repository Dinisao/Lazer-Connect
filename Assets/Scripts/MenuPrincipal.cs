using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ADICIONADO para poder usar o Slider

public class MenuPrincipal : MonoBehaviour
{
    [Header("Som")]
    public Slider sliderVolumeMenu; // Arraste o Slider do Menu Principal para aqui no Inspector

    void Start()
    {
        // 1. Vai buscar o volume guardado (se não houver, assume 100% que é 1f)
        float volumeGuardado = PlayerPrefs.GetFloat("VolumeJogo", 1f);

        // 2. Aplica o volume imediatamente ao barramento Master (isto já silencia/reduz a música do menu!)
        FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(volumeGuardado);

        // 3. Atualiza a posição visual do Slider no menu principal
        if (sliderVolumeMenu != null)
        {
            sliderVolumeMenu.value = volumeGuardado;

            // Adiciona o evento via código para garantir que ele responde quando mexes no Slider
            sliderVolumeMenu.onValueChanged.AddListener(AlterarVolumeMenu);
        }
    }

    public void AlterarVolumeMenu(float volume)
    {
        // Altera o som do FMOD em tempo real
        FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(volume);

        // Guarda para quando o jogador entrar nos níveis
        PlayerPrefs.SetFloat("VolumeJogo", volume);
    }

    public void BotaoPlayNormal()
    {
        SceneManager.LoadScene("JLevel 0");
    }

    public void AbrirMenuNiveis()
    {
        SceneManager.LoadScene("Menu Levels");
    }

    public void SairDoJogo()
    {
        Application.Quit();
    }
}