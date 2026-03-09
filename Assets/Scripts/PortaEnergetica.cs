using UnityEngine;

public class PortaEnergetica : MonoBehaviour
{
    private bool sinalRecebido = false;
    public Vector3 deslocamento = new Vector3(0, 5, 0);
    public float velocidade = 2f;

    private Vector3 posicaoInicial;
    private Vector3 posicaoAberta;

    void Start()
    {
        posicaoInicial = transform.position;
        posicaoAberta = posicaoInicial + deslocamento;
    }

    void Update()
    {
        // Se sinalRecebido for true, vai para a posição aberta; caso contrário, volta para a inicial
        Vector3 destino = sinalRecebido ? posicaoAberta : posicaoInicial;
        transform.position = Vector3.MoveTowards(transform.position, destino, velocidade * Time.deltaTime);

        // Resetamos o sinal no final de cada frame
        sinalRecebido = false;
    }

    // Esta é a função que o Laser vai chamar constantemente
    public void ManterAberta()
    {
        sinalRecebido = true;
    }
}