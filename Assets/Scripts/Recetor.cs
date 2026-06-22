using UnityEngine;

public class ReceiverVisual : MonoBehaviour
{
    public Texture texturaVermelha;
    public Texture texturaVerde;
    public Light luzRecetor;
    public Color corVermelha = Color.red;
    public Color corVerde = Color.green;

    private MeshRenderer meshRenderer;
    private bool atingido = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        meshRenderer.materials[0].mainTexture = atingido ? texturaVerde : texturaVermelha;

        if (luzRecetor != null)
            luzRecetor.color = atingido ? corVerde : corVermelha;

        atingido = false;
    }

    public void MarcarAtingido()
    {
        atingido = true;
    }
}