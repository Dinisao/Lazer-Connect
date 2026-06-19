using UnityEngine;
using System.Collections.Generic;
using FMODUnity; // IMPORTANTE: Adicionado para o Unity reconhecer o FMOD

[RequireComponent(typeof(LineRenderer))]
public class ControloLaser : MonoBehaviour
{
    public LineRenderer lineRenderer;

    [Header("Configurações do Laser")]
    public Transform pontoDisparo;
    public int maxReflexoes = 10;
    public float distanciaMaxima = 100f;
    public LayerMask camadasParaOcultar;

    [Tooltip("Distância da ponta do emissor onde o laser começa")]
    public float offsetSaidaLaser = 0.2f;

    [Tooltip("Espessura do laser")]
    public float larguraLaser = 0.2f;

    [Header("Estado")]
    public bool laserAtivo = false;

    [Header("Visual (Fix do Rosa)")]
    public Material materialDoLaser;

    [Header("Sons do FMOD (Contínuo)")]
    [Tooltip("O evento do FMOD para o som em loop do laser.")]
    public EventReference somLaserLoop;

    // --- O TEU NOVO SISTEMA CINEMÁTICO ---
    [Header("Controlo Nível 0 (Alarme -> Ambiente)")]
    [Tooltip("Ativa isto para o nível começar com alarme e sem ambiente")]
    public bool modoAlarmeInicial = false;
    public StudioEventEmitter emissorAlarme;   // O alarme que está a tocar no início
    public StudioEventEmitter emissorAmbiente; // A música que só entra quando ligas o laser

    // A variável que já tínhamos criado para segurar o teu Timer!
    public static bool primeiroDisparoFeito = false;
    // -------------------------------------

    // Instância privada para gerir o ciclo de vida do som contínuo
    private FMOD.Studio.EventInstance somLaserInstancia;

    private float limiteAlinhamento = 0.05f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Reseta o estado do timer sempre que o nível começa ou reinicia
        primeiroDisparoFeito = false;

        // Define a largura inicial baseada na variável pública
        lineRenderer.startWidth = larguraLaser;
        lineRenderer.endWidth = larguraLaser;

        if (materialDoLaser == null && lineRenderer.sharedMaterial != null)
        {
            materialDoLaser = lineRenderer.sharedMaterial;
        }

        // Se o laser começar já ativado, arranca tudo logo de início
        if (laserAtivo)
        {
            primeiroDisparoFeito = true;
            LigarSomLaser();
        }
    }

    void Update()
    {
        if (laserAtivo && pontoDisparo != null)
        {
            if (lineRenderer.sharedMaterial == null && materialDoLaser != null)
            {
                lineRenderer.material = materialDoLaser;
            }

            lineRenderer.startWidth = larguraLaser;
            lineRenderer.endWidth = larguraLaser;

            DesenharLaser();

            if (somLaserInstancia.isValid())
            {
                somLaserInstancia.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            }
        }
        else
        {
            if (lineRenderer.positionCount > 0) lineRenderer.positionCount = 0;
        }
    }

    void DesenharLaser()
    {
        List<Vector3> pontos = new List<Vector3>();
        Vector3 direcaoAtual = -pontoDisparo.right;
        Vector3 posicaoSaidaCorrigida = pontoDisparo.position + (direcaoAtual * offsetSaidaLaser);

        pontos.Add(posicaoSaidaCorrigida);
        Vector3 posicaoAtual = posicaoSaidaCorrigida;

        if (Mathf.Abs(direcaoAtual.y) < limiteAlinhamento)
        {
            direcaoAtual.y = 0;
            direcaoAtual.Normalize();
        }

        for (int i = 0; i < maxReflexoes; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(posicaoAtual, direcaoAtual, out hit, distanciaMaxima, ~camadasParaOcultar))
            {
                pontos.Add(hit.point);

                bool ehEspelho = hit.collider.CompareTag("Mirror") ||
                                 hit.collider.CompareTag("FixedMirror") ||
                                 (hit.collider.transform.parent != null &&
                                 (hit.collider.transform.parent.CompareTag("Mirror") || hit.collider.transform.parent.CompareTag("FixedMirror")));

                if (ehEspelho)
                {
                    direcaoAtual = Vector3.Reflect(direcaoAtual, hit.normal);

                    if (Mathf.Abs(direcaoAtual.y) < limiteAlinhamento)
                    {
                        direcaoAtual.y = 0;
                        direcaoAtual.Normalize();
                    }

                    posicaoAtual = hit.point + (hit.normal * 0.05f);
                }
                else if (hit.collider.CompareTag("Receiver"))
                {
                    PortaEnergetica porta = Object.FindFirstObjectByType<PortaEnergetica>();
                    if (porta != null) porta.ManterAberta();
                    break;
                }
                else break;
            }
            else
            {
                pontos.Add(posicaoAtual + (direcaoAtual * distanciaMaxima));
                break;
            }
        }

        lineRenderer.positionCount = pontos.Count;
        lineRenderer.SetPositions(pontos.ToArray());
    }

    public void AlternarLaser()
    {
        laserAtivo = !laserAtivo;
        if (!laserAtivo)
        {
            lineRenderer.positionCount = 0;
            PararSomLaser();
        }
        else
        {
            // SE FOR A PRIMEIRA VEZ QUE O JOGADOR LIGA O LASER:
            if (!primeiroDisparoFeito)
            {
                // 1. Liberta o Timer
                primeiroDisparoFeito = true;

                // 2. Faz a transição de áudio cinemática
                if (modoAlarmeInicial)
                {
                    // Cala o alarme para sempre
                    if (emissorAlarme != null) emissorAlarme.Stop();

                    // Começa a música ambiente
                    if (emissorAmbiente != null) emissorAmbiente.Play();
                }
            }

            LigarSomLaser();
        }
    }

    public void LigarSomLaser()
    {
        if (!somLaserLoop.IsNull && !somLaserInstancia.isValid())
        {
            somLaserInstancia = RuntimeManager.CreateInstance(somLaserLoop);
            somLaserInstancia.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            somLaserInstancia.start();
        }
    }

    public void PararSomLaser()
    {
        if (somLaserInstancia.isValid())
        {
            somLaserInstancia.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            somLaserInstancia.release();
        }
    }

    void OnDestroy()
    {
        PararSomLaser();
    }
}