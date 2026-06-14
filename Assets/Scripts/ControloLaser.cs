using UnityEngine;
using System.Collections.Generic;
using FMODUnity; // IMPORTANTE: Adicionado para o Unity reconhecer o FMOD

[RequireComponent(typeof(LineRenderer))]
public class ControloLaser : MonoBehaviour
{
    private LineRenderer lineRenderer;

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

    // --- NOVIDADE AQUI ---
    [Header("Controlo de Áudio FMOD (Ambiente)")]
    [Tooltip("Ativa isto APENAS no Nível 0 para calar o ambiente")]
    public bool isolarSomAmbiente = false;
    [Tooltip("Arrasta para aqui o objeto que tem o Studio Event Emitter do som da sala")]
    public StudioEventEmitter emissorAmbiente;
    // ---------------------

    // Instância privada para gerir o ciclo de vida do som contínuo
    private FMOD.Studio.EventInstance somLaserInstancia;

    private float limiteAlinhamento = 0.05f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Define a largura inicial baseada na variável pública
        lineRenderer.startWidth = larguraLaser;
        lineRenderer.endWidth = larguraLaser;

        if (materialDoLaser == null && lineRenderer.sharedMaterial != null)
        {
            materialDoLaser = lineRenderer.sharedMaterial;
        }

        // Se o laser começar já ativado, liga o som imediatamente
        if (laserAtivo)
        {
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

            // Garante que a largura se mantém atualizada caso alteres no Inspector em Runtime
            lineRenderer.startWidth = larguraLaser;
            lineRenderer.endWidth = larguraLaser;

            DesenharLaser();

            // Mantém a posição 3D do áudio colada ao emissor do laser
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

        // 1. Calculamos a direção (negativo do eixo X local do ponto de disparo)
        Vector3 direcaoAtual = -pontoDisparo.right;

        // 2. Aplicamos o Offset para o laser começar mais à frente da ponta
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

                    // Empurra o início da reflexão para fora do colisor para evitar auto-colisão
                    posicaoAtual = hit.point + (hit.normal * 0.05f);
                }
                else if (hit.collider.CompareTag("Receiver"))
                {
                    // Tenta encontrar a porta e mantê-la aberta
                    PortaEnergetica porta = Object.FindFirstObjectByType<PortaEnergetica>();
                    if (porta != null) porta.ManterAberta();
                    break;
                }
                else break;
            }
            else
            {
                // Se não bater em nada, estende o laser até à distância máxima
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
            PararSomLaser(); // Desliga o áudio se o laser for desativado nesta função
        }
        else
        {
            LigarSomLaser(); // Liga o áudio se o laser for ativado nesta função
        }
    }

    // Função interna e pública para iniciar o som de forma segura
    public void LigarSomLaser()
    {
        // --- NOVIDADE AQUI: CALA O AMBIENTE ---
        if (isolarSomAmbiente && emissorAmbiente != null)
        {
            emissorAmbiente.Stop();
        }

        if (!somLaserLoop.IsNull && !somLaserInstancia.isValid())
        {
            somLaserInstancia = RuntimeManager.CreateInstance(somLaserLoop);
            somLaserInstancia.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            somLaserInstancia.start();
        }
    }

    // Função interna e pública para silenciar o áudio imediatamente (usada também no TimerNivel)
    public void PararSomLaser()
    {
        // --- NOVIDADE AQUI: DEVOLVE O AMBIENTE ---
        if (isolarSomAmbiente && emissorAmbiente != null)
        {
            emissorAmbiente.Play();
        }

        if (somLaserInstancia.isValid())
        {
            somLaserInstancia.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            somLaserInstancia.release();
        }
    }

    // Segurança: se o emissor for destruído ou mudares de cena, limpa o som da RAM
    void OnDestroy()
    {
        PararSomLaser();
    }
}