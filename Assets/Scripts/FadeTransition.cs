using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeTransition : MonoBehaviour
{
    public string proximaCena; // Ex: "Cena2"
    public Image telaPreta;    // A tua imagem preta do Canvas da Cena 1

    [Header("Velocidade de Escurecer")]
    public float velocidadeFade = 1.5f;

    private bool jaComecou = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !jaAtivou())
        {
            jaComecou = true;
            StartCoroutine(FazerFadeECarregar());
        }
    }

    // Método auxiliar só para evitar o erro do "jaComecou" no IF
    private bool jaAtivou() { return jaComecou; }

    IEnumerator FazerFadeECarregar()
    {
        // 1. GARANTIR QUE A TELA PRETA ESTÁ LIGADA E TRANSPARENTE
        if (telaPreta != null)
        {
            telaPreta.gameObject.SetActive(true); // Liga a imagem caso a tenhas desligado no Editor
            Color cor = telaPreta.color;
            cor.a = 0f; // Começa invisível
            telaPreta.color = cor;

            // 2. ESCURECER SUAVEMENTE ATÉ AOS 100%
            while (telaPreta.color.a < 0.99f)
            {
                cor.a += Time.deltaTime * velocidadeFade;
                telaPreta.color = cor;
                yield return null; // Espera frame a frame
            }

            // Força a ficar totalmente opaco no final
            cor.a = 1f;
            telaPreta.color = cor;

            // 3. PAUSA DRAMÁTICA (Espera meio segundo no escuro)
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogError("Esqueceste-te de arrastar a Tela Preta para o script do elevador!");
        }

        // 4. SÓ AGORA CARREGA A CENA 2
        SceneManager.LoadSceneAsync(proximaCena);
    }
}