using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class ControloLaser : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Configurações do Laser")]
    public Transform pontoDisparo;
    public int maxReflexoes = 10;
    public float distanciaMaxima = 100f;
    public LayerMask camadasParaOcultar;

    [Header("Estado")]
    public bool laserAtivo = false;

    [Header("Visual (Fix do Rosa)")]
    public Material materialDoLaser; // Arraste o seu material para aqui no Inspector!

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Configuração inicial de largura
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        // Se tiveres esquecido de arrastar o material para o script, 
        // ele tenta pegar o que já está no componente LineRenderer
        if (materialDoLaser == null && lineRenderer.sharedMaterial != null)
        {
            materialDoLaser = lineRenderer.sharedMaterial;
        }
    }

    void Update()
    {
        if (laserAtivo && pontoDisparo != null)
        {
            // Garante que o material é aplicado antes de desenhar (evita o rosa)
            if (lineRenderer.sharedMaterial == null && materialDoLaser != null)
            {
                lineRenderer.material = materialDoLaser;
            }

            DesenharLaser();
        }
        else
        {
            if (lineRenderer.positionCount > 0) lineRenderer.positionCount = 0;
        }
    }

    void DesenharLaser()
    {
        List<Vector3> pontos = new List<Vector3>();
        pontos.Add(pontoDisparo.position);

        Vector3 posicaoAtual = pontoDisparo.position;
        Vector3 direcaoAtual = pontoDisparo.forward;

        for (int i = 0; i < maxReflexoes; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(posicaoAtual, direcaoAtual, out hit, distanciaMaxima, ~camadasParaOcultar))
            {
                pontos.Add(hit.point);

                // Verifica Tag Mirror ou FixedMirror no objeto ou no pai
                bool ehEspelho = hit.collider.CompareTag("Mirror") ||
                                 hit.collider.CompareTag("FixedMirror") ||
                                 (hit.collider.transform.parent != null &&
                                 (hit.collider.transform.parent.CompareTag("Mirror") || hit.collider.transform.parent.CompareTag("FixedMirror")));

                if (ehEspelho)
                {
                    direcaoAtual = Vector3.Reflect(direcaoAtual, hit.normal);
                    posicaoAtual = hit.point + (direcaoAtual * 0.01f);
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

        // Força a limpeza visual imediata ao desligar
        if (!laserAtivo) lineRenderer.positionCount = 0;
    }
}