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
    public Material materialDoLaser;

    // NOVO: Tolerância para alinhar o laser automaticamente
    private float limiteAlinhamento = 0.05f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        if (materialDoLaser == null && lineRenderer.sharedMaterial != null)
        {
            materialDoLaser = lineRenderer.sharedMaterial;
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

        // MAGIA INTELIGENTE: Se a inclinação inicial for um erro minúsculo, força a 0.
        // Mas se for uma inclinação a sério (espelho virado para o chão), deixa passar!
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

                    // MAGIA INTELIGENTE APÓS REFLETIR: Corrige ressaltos com erros milimétricos.
                    if (Mathf.Abs(direcaoAtual.y) < limiteAlinhamento)
                    {
                        direcaoAtual.y = 0;
                        direcaoAtual.Normalize();
                    }

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
        if (!laserAtivo) lineRenderer.positionCount = 0;
    }
}