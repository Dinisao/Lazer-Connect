using UnityEngine;

public class MirrorRotate : MonoBehaviour
{
    public void Rotate()
    {
        // Space.Self garante que roda sobre o seu próprio eixo Z (o pai)
        transform.Rotate(0, 0, 45, Space.Self);
    }
}