using UnityEngine;
using FMODUnity;

public class SonsPassosFMOD : MonoBehaviour
{
    public enum TipoSuperficie
    {
        Azulejo = 0,
        Metal = 1,
        Terra = 2
    }

    [Header("Configuração FMOD")]
    public EventReference eventoPassos;

    [Tooltip("Nome exato do parâmetro criado no FMOD Studio")]
    public string nomeParametroFMOD = "Superficies";

    [Header("Escolha do Terreno")]
    public TipoSuperficie superficieAtual = TipoSuperficie.Metal;

    [Header("Intervalos do Passo (Segundos)")]
    public float intervaloAndar = 0.5f;
    public float intervaloCorrer = 0.3f;

    [Header("Velocidade para Detetar Corrida")]
    public float limiteVelocidadeCorrida = 5.5f;

    private CharacterController controller;
    private float timerPasso;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller == null) return;

        Vector3 velocidadeHorizontal = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        float magnitudeVelocidade = velocidadeHorizontal.magnitude;

        if (controller.isGrounded && magnitudeVelocidade > 0.2f)
        {
            bool estaACorrer = magnitudeVelocidade >= limiteVelocidadeCorrida;
            float intervaloAtual = estaACorrer ? intervaloCorrer : intervaloAndar;

            timerPasso += Time.deltaTime;

            if (timerPasso >= intervaloAtual)
            {
                TocarPassoFMOD();
                timerPasso = 0f;
            }
        }
        else
        {
            timerPasso = 0f;
        }
    }

    void TocarPassoFMOD()
    {
        if (eventoPassos.IsNull) return;

        FMOD.Studio.EventInstance instanciaPasso = RuntimeManager.CreateInstance(eventoPassos);

        instanciaPasso.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        instanciaPasso.setParameterByName(nomeParametroFMOD, (float)superficieAtual);

        instanciaPasso.start();
        instanciaPasso.release();
    }
}