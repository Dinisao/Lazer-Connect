using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
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