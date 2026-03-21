using UnityEngine;
using System.Collections.Generic;

public class ControloLaser : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform pontoDisparo;
    public int maxReflexoes = 5;
    public float distanciaMaxima = 100f;
    public LayerMask camadasParaOcultar;

    public bool laserAtivo = false; // Opção no Inspector

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (laserAtivo && lineRenderer != null) DesenharLaser();
        else if (lineRenderer != null) lineRenderer.positionCount = 0;
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

                if (hit.collider.CompareTag("Mirror") || hit.collider.CompareTag("FixedMirror"))
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

    public void AlternarLaser() { laserAtivo = !laserAtivo; }
}