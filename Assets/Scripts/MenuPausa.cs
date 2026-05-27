using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;

public class MenuPausa : MonoBehaviour
{
    [Header("UI")]
    public GameObject painelPausa;
    public Slider sliderVolume;

    [Header("Configurações")]
    public string nomeCenaMenuPrincipal = "MenuPrincipal";

    public static bool jogoPausado = false;

    void Start()
    {
        painelPausa.SetActive(false);
        jogoPausado = false;

        // Garante que o som arranca despausado quando a cena carrega
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false);

        if (sliderVolume != null)
        {
            sliderVolume.value = 1f;
        }
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (jogoPausado) ContinuarJogo();
            else PausarJogo();
        }
    }

    public void PausarJogo()
    {
        painelPausa.SetActive(true);
        Time.timeScale = 0f;
        jogoPausado = true;

        // PÁRA O SOM DO FMOD! Congela o Master Bus inteiro.
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(true);

        var inputs = Object.FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.cursorLocked = false;
            inputs.cursorInputForLook = false;
        }

        var controller = Object.FindFirstObjectByType<FirstPersonController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ContinuarJogo()
    {
        painelPausa.SetActive(false);
        Time.timeScale = 1f;
        jogoPausado = false;

        // DESCONGELA O SOM DO FMOD!
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false);

        var inputs = Object.FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.cursorLocked = true;
            inputs.cursorInputForLook = true;
        }

        var controller = Object.FindFirstObjectByType<FirstPersonController>();
        if (controller != null)
        {
            controller.enabled = true;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ReiniciarNivel()
    {
        ContinuarJogo(); // O ContinuarJogo já despausa o som e a câmara automaticamente!
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IrParaMenu()
    {
        ContinuarJogo();
        SceneManager.LoadScene(nomeCenaMenuPrincipal);
    }

    public void AlterarVolume(float volume)
    {
        FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(volume);
    }
}