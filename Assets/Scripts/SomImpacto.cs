using UnityEngine;
using FMODUnity;

public class SomImpacto : MonoBehaviour
{
    [Header("Som FMOD")]
    [Tooltip("O som que vai tocar quando este objeto bater em algo.")]
    public EventReference somBaterNoChao;

    [Header("Configurações")]
    [Tooltip("Força mínima da pancada para o som tocar (evita que toque a toda a hora se estiver só a raspar no chão)")]
    public float forcaMinima = 1.5f;

    // Esta função do Unity é chamada automaticamente sempre que a física do objeto bate em algo
    private void OnCollisionEnter(Collision collision)
    {
        // Verifica se o objeto bateu com força suficiente
        if (collision.relativeVelocity.magnitude >= forcaMinima)
        {
            // Toca o som do FMOD na posição exata deste objeto
            if (!somBaterNoChao.IsNull)
            {
                RuntimeManager.PlayOneShot(somBaterNoChao, transform.position);
            }
        }
    }
}