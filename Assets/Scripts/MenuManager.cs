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
        // Garante que o jogo começa no painel certo
        MostrarPainelPrincipal();

        // 1. Verifica o progresso guardado
        nivelMaximoAlcancado = PlayerPrefs.GetInt("NivelDesbloqueado", 0);

        // 2. Tranca ou destranca os botões
        for (int i = 0; i < botoesDeNivel.Length; i++)
        {
            if (i > nivelMaximoAlcancado)
            {
                botoesDeNivel[i].interactable = false; // Bloqueado
            }
            else
            {
                botoesDeNivel[i].interactable = true; // Desbloqueado
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
        SceneManager.LoadScene("Level 0");
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
}