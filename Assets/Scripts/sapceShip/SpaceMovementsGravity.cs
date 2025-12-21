using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class SpaceMovementsGravity : MonoBehaviour
{
    [Header("parametres")]
    public float puissance;
    public GameObject player;//le joueur
    public bool burning;
 

    //variables d instances
    public dataHolder data;
    NbodySimulation Simulation;
    Rigidbody rb;
    constant constantValues;
    [HideInInspector] public FirstPersonController firstPersonController;
    public CelestialBody reference;


    [Header("déplacements")]
    //mouvement
    [HideInInspector] public Vector3 moveAmount;
    Vector3 smoothMoveVelocity; //utiliser pour avoir une acceleration smooth 
    public float speedMovement;
    Vector3 velocity;
    //rotation
    public float rotationSpeed = 1f;
    Quaternion targetRotation = Quaternion.identity;
    Quaternion smoothRot =  Quaternion.identity;


    //reference au sol
    GameObject referenceGround;
    bool Grounded;

    [Header("VFX")]
    public ParticleSystem gaz;//propulsion

    [Header("FX")]
    SoundMaker burningSound;


    private void Start()
    {
        //recuperation des variables d instances
        Simulation = GameObject.Find("Universe").GetComponent<NbodySimulation>();//tous les corps
        constantValues = GameObject.Find("Universe").GetComponent<constant>();//les constantes (gravitation)
        firstPersonController = player.GetComponent<FirstPersonController>();//le controleur du joueur
        rb = GetComponent<Rigidbody>();//Rigidbody 

        //recuperer les sons
        foreach (SoundMaker sound in data.soundsManager.sounds)
        {
            if (sound.soundName == "rocket_burning")
            {
                burningSound = sound;
            }
        }
        
    }

    private void Update()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 targetMoveAmount =  moveDirection * speedMovement;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

        if (Input.GetButton("Jump") && data.inSpaceShip){
            float distanceBetweenRef = Vector3.Distance(transform.position, reference ? reference.transform.position : Vector3.zero);
            burningSound.play = true;
            burning = true;
            gaz.Play();
        }
        else
        {
            burningSound.play = false;
            burning = false;
            gaz.Stop();
        }

        Grounded = false;
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2f))
        {
            Grounded = true;
            referenceGround = hit.collider.gameObject;
        }
        else
        {
            referenceGround = null;
        }
        Debug.DrawRay(transform.position, -transform.up);

        if (data.inSpaceShip)
        {
            HandleRotation();
        }
        /*
        // Rotation horizontale (autour de Y)
        float mouseX = Input.GetAxis("Mouse X") * firstPersonController.mouseSensitivityX * Time.deltaTime;
        Quaternion rotationY = Quaternion.Euler(0f, mouseX, 0f).normalized;

        // Rotation verticale (autour de X)
        float mouseY = Input.GetAxis("Mouse Y") * firstPersonController.mouseSensitivityY * Time.deltaTime;
        Quaternion rotationX = Quaternion.Euler(-mouseY, 0f, 0f).normalized;

        //rotation frontale (autour de Z)
        float rotateZ = ((Input.GetKey(KeyCode.Q) ? 1f : 0f) - (Input.GetKey(KeyCode.E) ? 1f : 0f)) * 50f * Time.deltaTime; //qwerty (50f = sensi)
        
        Quaternion rotationZ = Quaternion.Euler(0f,0f,rotateZ).normalized;
        Quaternion targetRotation = rotationX * rotationY * rotationZ;
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(rb.rotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        // normalisation de l'angle pour être en radians
        float torqueStrength = 10f; // à ajuster selon ton besoin
        Vector3 torque = axis * angle * Mathf.Deg2Rad * torqueStrength;

        // appliquer le torque
        rb.AddTorque(torque, ForceMode.Impulse);
        //rb.MoveRotation(rb.rotation * rotation);
        */


    }



    private void LateUpdate()
    {
        CameraMovement();
    }

    private void FixedUpdate()
    {
        if (burning)
            rb.AddForce(transform.forward * puissance);
        CelestialBody[] bodies = Simulation.bodies;
        Vector3 strongestGravitionalPull = Vector3.zero;

        if (data.firstPersonController.influenceByBody(transform, reference))
        {
            Vector3 spaceShipMove = Vector3.zero;
            if (data.inSpaceShip)
                spaceShipMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
            Vector3 planetMove = reference.currentVelocity * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + spaceShipMove + planetMove);
        }


        //Gravity
        foreach (CelestialBody body in bodies)
        {
            float sqrtDst = (body.transform.position - rb.position).sqrMagnitude;
            Vector3 forceDir = (body.transform.position - rb.position).normalized;
            Vector3 acceleration = (forceDir * constantValues.GravityConstant * body.mass / sqrtDst);

            
            rb.AddForce(acceleration, ForceMode.Acceleration);

            //Find Body with strongest gravitanional pull
            if (acceleration.sqrMagnitude > strongestGravitionalPull.sqrMagnitude)
            { 
                strongestGravitionalPull = acceleration;
                reference = body;

            }

            

        }

        

        //rb.MoveRotation(smoothRot);

        
        

    }

    void CameraMovement()
    {
        if (!data.inSpaceShip)
            return;
        Quaternion axeYRotation = Quaternion.Euler(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * data.mouseSensitivityX * rotationSpeed);
        rb.MoveRotation(rb.rotation * axeYRotation);

        
        Quaternion axeZRotation = Quaternion.Euler(Vector3.left * Input.GetAxis("Mouse Y") * Time.deltaTime * data.mouseSensitivityY * rotationSpeed);

        rb.MoveRotation(rb.rotation * axeZRotation);

    }

    void HandleRotation()
    {
        float yawInput = Input.GetAxisRaw("Mouse X") * data.mouseSensitivityX * rotationSpeed / 100;
        float pitchInput = Input.GetAxisRaw("Mouse Y") * data.mouseSensitivityY * rotationSpeed / 100;
        float rollInput = ((Input.GetKey(KeyCode.Q) ? 1f : 0f) - (Input.GetKey(KeyCode.E) ? 1f : 0f)) * 50f * Time.deltaTime; //qwerty (50f = sensi)

        //Calculate rotation
        var yaw = Quaternion.AngleAxis(yawInput, transform.up);
        var pitch = Quaternion.AngleAxis(-pitchInput, transform.right);
        var roll = Quaternion.AngleAxis(-rollInput, transform.forward);

        targetRotation = yaw * pitch * roll * targetRotation;

        smoothRot = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 100f);//10f smooth rotation Speed


    }



}
