using UnityEngine;
using System.Collections.Generic;

public class LaserLight : MonoBehaviour
{
    public ControloLaser laser;
    public Light prefabLuz; // Cria um prefab de Point Light vermelha
    public int numeroLuzesMinimo = 3;
    public int numeroLuzesMaximo = 8;
    public float distanciaPorLuz = 15f; // 1 luz a cada X metros de percurso

    private List<Light> luzesAtivas = new List<Light>();

    void Update()
    {
        if (laser == null || prefabLuz == null) return;

        if (!laser.laserAtivo)
        {
            DesligarTodasLuzes();
            return;
        }

        if (laser.lineRenderer == null || laser.lineRenderer.positionCount < 2)
        {
            DesligarTodasLuzes();
            return;
        }

        AtualizarLuzes();
    }

    void AtualizarLuzes()
    {
        // Calcula o comprimento total do laser
        float comprimentoTotal = 0f;
        int totalPontos = laser.lineRenderer.positionCount;

        for (int i = 0; i < totalPontos - 1; i++)
        {
            comprimentoTotal += Vector3.Distance(
                laser.lineRenderer.GetPosition(i),
                laser.lineRenderer.GetPosition(i + 1)
            );
        }

        // Decide quantas luzes precisa baseado na distância total
        int numeroLuzes = Mathf.Clamp(
            Mathf.CeilToInt(comprimentoTotal / distanciaPorLuz),
            numeroLuzesMinimo,
            numeroLuzesMaximo
        );

        // Cria luzes novas se precisar de mais
        while (luzesAtivas.Count < numeroLuzes)
        {
            Light novaLuz = Instantiate(prefabLuz, transform);
            luzesAtivas.Add(novaLuz);
        }

        // Desativa luzes extra se precisar de menos
        for (int i = 0; i < luzesAtivas.Count; i++)
        {
            luzesAtivas[i].gameObject.SetActive(i < numeroLuzes);
        }

        // Distribui as luzes ativas uniformemente ao longo do percurso
        for (int i = 0; i < numeroLuzes; i++)
        {
            float percentagem = (i + 0.5f) / numeroLuzes; // +0.5 para centrar no segmento
            Vector3 posicao = CalcularPosicaoNoPercurso(percentagem * comprimentoTotal);
            luzesAtivas[i].transform.position = posicao;
        }
    }

    Vector3 CalcularPosicaoNoPercurso(float distanciaAlvo)
    {
        int totalPontos = laser.lineRenderer.positionCount;
        float percorrido = 0f;

        for (int i = 0; i < totalPontos - 1; i++)
        {
            Vector3 pontoA = laser.lineRenderer.GetPosition(i);
            Vector3 pontoB = laser.lineRenderer.GetPosition(i + 1);
            float distSegmento = Vector3.Distance(pontoA, pontoB);

            if (percorrido + distSegmento >= distanciaAlvo)
            {
                float t = (distanciaAlvo - percorrido) / distSegmento;
                return Vector3.Lerp(pontoA, pontoB, t);
            }

            percorrido += distSegmento;
        }

        return laser.lineRenderer.GetPosition(totalPontos - 1);
    }

    void DesligarTodasLuzes()
    {
        foreach (Light l in luzesAtivas)
        {
            if (l != null) l.gameObject.SetActive(false);
        }
    }
}