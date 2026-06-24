using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    void Awake()
    {
        // Sistema Singleton: impede que se criem clones da música ao voltar ao menu
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // DIZ AO UNITY PARA NÃO DESTRUIR A MÚSICA AO MUDAR DE CENA
        DontDestroyOnLoad(gameObject);
    }
}