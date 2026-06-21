using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Seleção de Níveis")]
    public Button[] botoesDeNivel;

    [Header("Paginação")]
    public GameObject[] paginas;
    private int paginaAtual = 0;

    [Header("Navegação")]
    public string nomeCenaMenuPrincipal = "Menu";

    private int nivelMaximoAlcancado;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        nivelMaximoAlcancado = PlayerPrefs.GetInt("NivelDesbloqueado", 0);

        MostrarPagina(0);
    }

    void MostrarPagina(int index)
    {
        for (int i = 0; i < paginas.Length; i++)
            paginas[i].SetActive(i == index);
        paginaAtual = index;
    }

    public void ProximaPagina()
    {
        if (paginaAtual < paginas.Length - 1)
            MostrarPagina(paginaAtual + 1);
    }

    public void PaginaAnterior()
    {
        if (paginaAtual > 0)
            MostrarPagina(paginaAtual - 1);
    }

    public void EscolherNivelEspecifico(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }

    public void VoltarAoMenuPrincipal()
    {
        SceneManager.LoadScene(nomeCenaMenuPrincipal);
    }

    public void ApagarTodoOProgresso()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}