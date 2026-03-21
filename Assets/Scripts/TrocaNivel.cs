using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocaNivel : MonoBehaviour
{
    [Header("Configurações")]
    public string nomeDoProximoNivel; // Este é o valor que o Inspector vai usar

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto tem a Tag "Player"
        if (other.CompareTag("Player"))
        {
            // Usamos a variável 'nomeDoProximoNivel' em vez de escrever "Level 2"
            if (!string.IsNullOrEmpty(nomeDoProximoNivel))
            {
                SceneManager.LoadScene(nomeDoProximoNivel);
            }
            else
            {
                Debug.LogWarning("Escreve o nome do nível no Inspector!");
            }
        }
    }
}