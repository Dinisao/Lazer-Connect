using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class RolarCreditos : MonoBehaviour
{
    [Header("Configurações do Texto")]
    public RectTransform textoCreditos;
    public float velocidade = 100f; // Velocidade a que o texto sobe
    public float limiteY = 2500f;   // A altura em que o texto desaparece no topo e o jogo volta ao menu

    [Header("Navegação")]
    public string nomeCenaMenu = "MenuPrincipal";

    void Start()
    {
        // Garante que o rato volta a aparecer se quiseres clicar em algo no menu depois
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        // Empurra o texto para cima todos os frames
        textoCreditos.anchoredPosition += Vector2.up * velocidade * Time.deltaTime;

        // Se o texto chegar ao topo OU se o jogador carregar no ESC ou Espaço para saltar os créditos
        if (textoCreditos.anchoredPosition.y >= limiteY || Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            VoltarAoMenu();
        }
    }

    void VoltarAoMenu()
    {
        SceneManager.LoadScene(nomeCenaMenu);
    }
}