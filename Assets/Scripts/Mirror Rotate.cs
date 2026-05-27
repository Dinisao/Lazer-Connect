using UnityEngine;

public class MirrorRotate : MonoBehaviour
{
    [Header("Arraste aqui APENAS o objeto da Base deste espelho")]
    public Transform objetoBase;

    [Header("Configuração de Rotação")]
    public float anguloRotacao = 90f;

    public void Rotate()
    {
        // 1. Se este espelho tiver uma base e ela for filha dele (espelho de pegar)
        if (objetoBase != null && objetoBase.IsChildOf(transform))
        {
            // Tiramos a base temporariamente do "saco" para ela não rodar
            Transform paiOriginal = objetoBase.parent;
            objetoBase.SetParent(null);

            // Roda o pai como um volante perfeito (Mesh e Colisor rodam juntos sem quebrar o laser)
            transform.Rotate(Vector3.forward, anguloRotacao, Space.Self);

            // Devolvemos a base para dentro do saco para o jogador a poder levar na mão
            objetoBase.SetParent(paiOriginal);
        }
        else
        {
            // 2. Se for um espelho fixo (a base já está fora), roda normal como um volante
            transform.Rotate(Vector3.forward, anguloRotacao, Space.Self);
        }
    }
}