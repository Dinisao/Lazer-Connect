using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Os Teus Painéis")]
    public GameObject painelPrincipal;
    public GameObject painelNiveis;

    [Header("Seleção de Níveis")]
    [Tooltip("Arrasta os botões do nível 0, 1, 2, etc. por ordem para aqui")]
    public Button[] botoesDeNivel;

    private int nivelMaximoAlcancado;

    void Start()
    {
        // --- ESTAS DUAS LINHAS RESOLVEM O PROBLEMA ---
        Cursor.lockState = CursorLockMode.None; // Liberta o rato para se mover
        Cursor.visible = true;                  // Torna o rato visível outra vez
                                                // ----------------------------------------------

        // Daqui para baixo continua o teu código original...
        MostrarPainelPrincipal();

        nivelMaximoAlcancado = PlayerPrefs.GetInt("NivelDesbloqueado", 0);

        for (int i = 0; i < botoesDeNivel.Length; i++)
        {
            if (i > nivelMaximoAlcancado)
            {
                botoesDeNivel[i].interactable = false;
            }
            else
            {
                botoesDeNivel[i].interactable = true;
            }
        }
    }

    // --- NOVAS FUNÇÕES PARA TROCAR DE PAINEL ---

    // Liga ao teu novo botão "Levels"
    public void MostrarPainelNiveis()
    {
        painelPrincipal.SetActive(false); // Esconde o principal
        painelNiveis.SetActive(true);     // Mostra os níveis
    }

    // Liga ao teu botão "Voltar" (dentro do painel de níveis)
    public void MostrarPainelPrincipal()
    {
        painelPrincipal.SetActive(true);  // Mostra o principal
        painelNiveis.SetActive(false);    // Esconde os níveis
    }

    // --- FUNÇÕES ANTIGAS (MANTIDAS) ---

    public void BotaoPlayNormal()
    {
        SceneManager.LoadScene("JLevel 0");
    }

    public void EscolherNivelEspecifico(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }

    public void ApagarTodoOProgresso()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- FUNÇÃO DE SAIR ---

    public void SairDoJogo()
    {
        Debug.Log("O jogador saiu do jogo!");
        Application.Quit();
    }
}