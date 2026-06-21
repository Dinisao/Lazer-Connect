using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;
using TMPro;

public class MenuPausa : MonoBehaviour
{
    [Header("UI")]
    public GameObject painelPausa;
    public Slider sliderVolume;
    public TextMeshProUGUI tituloPausa;

    [Header("Game Over")]
    public GameObject botaoResume;

    [Header("Configurações")]
    public string nomeCenaMenuPrincipal = "Menu";
    public static bool jogoPausado = false;

    void Start()
    {
        painelPausa.SetActive(false);
        jogoPausado = false;

        float volumeGuardado = PlayerPrefs.GetFloat("VolumeJogo", 1f);
        FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(volumeGuardado);

        if (sliderVolume != null)
            sliderVolume.value = volumeGuardado;
    }

    void Update()
    {
        if (jogoPausado && botaoResume != null && !botaoResume.activeSelf) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (jogoPausado) ContinuarJogo();
            else PausarJogo();
        }
    }

    public void MostrarGameOver()
    {
        painelPausa.SetActive(true);
        jogoPausado = true;

        if (botaoResume != null)
            botaoResume.SetActive(false);

        if (tituloPausa != null)
            tituloPausa.text = "The Reactor Exploded!";

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void PausarJogo()
    {
        if (botaoResume != null)
            botaoResume.SetActive(true);
        if (sliderVolume != null)
            sliderVolume.gameObject.SetActive(true);
        if (tituloPausa != null)
            tituloPausa.text = "Pausa";

        painelPausa.SetActive(true);
        Time.timeScale = 0f;
        jogoPausado = true;
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(true);

        var inputs = Object.FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.cursorLocked = false;
            inputs.cursorInputForLook = false;
        }
        var controller = Object.FindFirstObjectByType<FirstPersonController>();
        if (controller != null) controller.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ContinuarJogo()
    {
        painelPausa.SetActive(false);
        Time.timeScale = 1f;
        jogoPausado = false;
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false);

        var inputs = Object.FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.cursorLocked = true;
            inputs.cursorInputForLook = true;
        }
        var controller = Object.FindFirstObjectByType<FirstPersonController>();
        if (controller != null) controller.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        jogoPausado = false;
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false);
        if (botaoResume != null)
            botaoResume.SetActive(true);
        if (sliderVolume != null)
            sliderVolume.gameObject.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IrParaMenu()
    {
        Time.timeScale = 1f;
        jogoPausado = false;
        FMODUnity.RuntimeManager.GetBus("bus:/").setPaused(false);
        if (botaoResume != null)
            botaoResume.SetActive(true);
        if (sliderVolume != null)
            sliderVolume.gameObject.SetActive(true);
        SceneManager.LoadScene(nomeCenaMenuPrincipal);
    }

    public void AlterarVolume(float volume)
    {
        FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(volume);
        PlayerPrefs.SetFloat("VolumeJogo", volume);
    }
}