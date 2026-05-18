using UnityEngine;

public class PortaEnergetica : MonoBehaviour
{
    private bool sinalRecebido = false;
    public Vector3 deslocamento = new Vector3(0, 5, 0);
    public float velocidade = 2f;

    [Header("Configuração de Áudio (Fmod)")]
    // CORREÇÃO: Removeu-se o [EventRef] antigo e mudou-se o tipo para EventReference
    public FMODUnity.EventReference eventoSomPorta;
    private bool estavaAberta = false;

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

        // Deteta a mudança de estado para tocar o som do FMOD apenas uma vez
        if (sinalRecebido != estavaAberta)
        {
            TocarSomFmod();
            estavaAberta = sinalRecebido;
        }

        // Resetamos o sinal no final de cada frame
        sinalRecebido = false;
    }

    // Esta é a função que o Laser vai chamar constantemente
    public void ManterAberta()
    {
        sinalRecebido = true;
    }

    // Chamada do evento do FMOD na posição real da porta
    private void TocarSomFmod()
    {
        // CORREÇÃO: EventReference agora usa .IsNull para verificar se está vazia
        if (!eventoSomPorta.IsNull)
        {
            // Dispara o som em 3D usando a posição atual da porta
            FMODUnity.RuntimeManager.PlayOneShot(eventoSomPorta, transform.position);
        }
    }
}