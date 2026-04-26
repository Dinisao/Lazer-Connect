using UnityEngine;
using UnityEngine.InputSystem;

public class PegarCaixas : MonoBehaviour
{
    [Header("Configurações")]
    public float distanciaPegar = 3f;

    [Header("Ajuste Fino")]
    [Tooltip("Os valores que guardaste ficam aqui aplicados permanentemente.")]
    public Vector3 offsetNoEcra = new Vector3(0, 0, 0);

    [Header("Referências")]
    public Camera cameraJogador;
    public Transform posicaoSegurar;

    private GameObject caixaSegurada;
    private Rigidbody rbCaixa;
    private Collider colliderCaixa;

    void Update()
    {
        if (Keyboard.current == null) return;

        // 1. Pegar e largar apenas com a tecla E
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (caixaSegurada == null) TentarPegar();
            else Largar();
        }

        // 2. Aplicar a posição com o offset guardado (sem os controlos das setas!)
        if (caixaSegurada != null)
        {
            caixaSegurada.transform.position = posicaoSegurar.position +
                posicaoSegurar.right * offsetNoEcra.x +
                posicaoSegurar.up * offsetNoEcra.y +
                posicaoSegurar.forward * offsetNoEcra.z;

            caixaSegurada.transform.rotation = posicaoSegurar.rotation;
        }
    }

    void TentarPegar()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraJogador.transform.position, cameraJogador.transform.forward, out hit, distanciaPegar))
        {
            if (hit.transform.CompareTag("Caixa"))
            {
                caixaSegurada = hit.transform.gameObject;
                rbCaixa = caixaSegurada.GetComponent<Rigidbody>();
                colliderCaixa = caixaSegurada.GetComponent<Collider>();

                // Desliga a física e as colisões
                if (rbCaixa != null) { rbCaixa.isKinematic = true; rbCaixa.useGravity = false; }
                if (colliderCaixa != null) colliderCaixa.enabled = false;
            }
        }
    }

    void Largar()
    {
        if (caixaSegurada == null) return;

        // Liga a física e as colisões de volta
        if (rbCaixa != null) { rbCaixa.isKinematic = false; rbCaixa.useGravity = true; }
        if (colliderCaixa != null) colliderCaixa.enabled = true;

        caixaSegurada = null;
    }
}