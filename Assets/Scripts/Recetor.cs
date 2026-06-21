using UnityEngine;

public class ReceiverVisual : MonoBehaviour
{
    public Texture texturaVermelha;
    public Texture texturaVerde;

    private MeshRenderer meshRenderer;
    private bool atingido = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        meshRenderer.materials[0].mainTexture = atingido ? texturaVerde : texturaVermelha;
        atingido = false;
    }

    public void MarcarAtingido()
    {
        atingido = true;
    }
}