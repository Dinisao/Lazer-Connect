using UnityEngine;

public class InterruptorLaser : MonoBehaviour
{
    [Header("Referências do Laser")]
    public ControloLaser scriptLaser; // Onde vais arrastar o teu Emissor

    [Header("Materiais de Visual")]
    public Material materialON;  // Onde vais arrastar o teu material verde ON
    public Material materialOFF; // Onde vais arrastar o teu material vermelho OFF

    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        AtualizarVisualInterruptor();
    }

    // Esta função deve ser chamada quando o teu jogador clica/interage com o botão
    public void InteragirComInterruptor()
    {
        if (scriptLaser == null) return;

        // Chama a função exata do teu script para ligar/desligar o laser e o áudio
        scriptLaser.AlternarLaser();

        // Altera as texturas do modelo 3D
        AtualizarVisualInterruptor();
    }

    void AtualizarVisualInterruptor()
    {
        if (meshRenderer == null || materialON == null || materialOFF == null || scriptLaser == null) return;

        // Copia a lista de materiais do MeshRenderer
        Material[] materiaisAtuais = meshRenderer.materials;

        // Modifica apenas o Element 0 (a tua textura de texto OFF/ON) baseado no teu bool 'laserAtivo'
        if (scriptLaser.laserAtivo)
        {
            materiaisAtuais[0] = materialON;
        }
        else
        {
            materiaisAtuais[0] = materialOFF;
        }

        // Devolve os materiais atualizados ao objeto
        meshRenderer.materials = materiaisAtuais;
    }
}