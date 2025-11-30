using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("infos")]//tous les parametres debug lié à la simulation
    public Text HeightText;
    public Text Reference;
    public Text GroundReference;
    public Text Velocity;
    public Text RelativeVelocity; //speed on the reference object
    public Text movementType;
    public Text FPS;


    //variables d instances
    FirstPersonController firstPersonController;
    Rigidbody rb;

    Vector3 lastPosition;
    float speed;
    float fps;


    private void Start()
    {
        firstPersonController = GetComponent<FirstPersonController>(); 
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
    }

    float GetVelocity(Vector3 lastPosition, Vector3 actualPosition)
    {
        float distance = Vector3.Distance(actualPosition, lastPosition);

        // Vitesse = distance / temps écoulé
        float rawSpeed = distance / Time.deltaTime;

        // Lissage
        speed = Mathf.Lerp(speed, rawSpeed, 0.1f);

        return speed;
    }


    float GetFps(float deltaTime)
    {
        float rawFps = 1 / deltaTime;
        fps = Mathf.Lerp(fps, rawFps, .1f);

        return fps;
    }


    private void Update()
    {
        CelestialBody reference = firstPersonController.reference;
        Reference.text = reference ? reference.name : "null";

        GameObject groundreference = firstPersonController.referenceGround;
        GroundReference.text = groundreference ? "land on " + groundreference.name : "not landed";

        float distanceBetweenRef = Vector3.Distance(transform.position, reference ? reference.transform.position : Vector3.zero);
        HeightText.text = (int)distanceBetweenRef + " m";


        Velocity.text = (int)GetVelocity(lastPosition, transform.position) + " m/s";

        RelativeVelocity.text = reference ? reference.name + " Speed " + (int)reference.GetComponent<CelestialBody>().currentVelocity.sqrMagnitude + " m/s" : "null";

        movementType.text = firstPersonController.SpaceMovement ? "mouvement spatial" : "mouvement planetaire";

        
        FPS.text = (int)GetFps(Time.deltaTime) + " FPS";

        lastPosition = transform.position;

    }
}
