using UnityEngine;
using System.Collections.Generic;

public class ControloLaser : MonoBehaviour
{
    private LineRenderer lr;
    public bool estaLigado = true;
    public int maxReflexoes = 10;
    public float alcanceMaximo = 1000f;

    [Header("Filtro")]
    [Tooltip("Selecione aqui a Layer do PLAYER e a Layer do PAI do espelho")]
    public LayerMask camadasParaIgnorar;

    void Start() { lr = GetComponent<LineRenderer>(); }

    void Update()
    {
        if (estaLigado && lr != null) { lr.enabled = true; DesenharLaser(); }
        else if (lr != null) lr.enabled = false;
    }

    public void AlternarLaser() { estaLigado = !estaLigado; }

    void DesenharLaser()
    {
        Vector3 origem = transform.position;
        Vector3 direcao = transform.forward;
        List<Vector3> pontos = new List<Vector3> { origem };

        for (int i = 0; i < maxReflexoes; i++)
        {
            Ray raio = new Ray(origem, direcao);
            RaycastHit hit;

            // Atravessa o Player e o "caixote" do Pai, mas bate no modelo visual (Filho)
            if (Physics.Raycast(raio, out hit, alcanceMaximo, ~camadasParaIgnorar))
            {
                pontos.Add(hit.point);

                if (hit.collider.CompareTag("Mirror") || hit.collider.CompareTag("FixedMirror"))
                {
                    // Reflete no ângulo exato do modelo visual
                    direcao = Vector3.Reflect(direcao, hit.normal);
                    origem = hit.point + (direcao * 0.02f);
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
                pontos.Add(origem + (direcao * alcanceMaximo));
                break;
            }
        }
        lr.positionCount = pontos.Count;
        lr.SetPositions(pontos.ToArray());
    }
}