using Unity.VisualScripting;
using UnityEngine;

public class SpaceMovementsGravity : MonoBehaviour
{
    NbodySimulation Simulation;
    Rigidbody rb;
    constant constantValues;
    FirstPersonController firstPersonController;
    [HideInInspector] public CelestialBody reference;

    //deplacements
    [HideInInspector] public Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    GameObject player;
    GameObject referenceGround;
    bool Grounded;

    //VFX
    [HideInInspector] public ParticleSystem gaz;

    public float puissance;




    private void Start()
    {
        //init player
        player = GameObject.FindGameObjectWithTag("Player");
        Simulation = GameObject.Find("Universe").GetComponent<NbodySimulation>();
        constantValues = GameObject.Find("Universe").GetComponent<constant>();
        firstPersonController = player.GetComponent<FirstPersonController>();
        gaz = GetComponentInChildren<ParticleSystem>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 targetMoveAmount =  moveDirection * 10f;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

        float rotateInput = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f); //qwerty 
        transform.Rotate(Vector3.forward * rotateInput * 50f * Time.deltaTime);

        

        if (Input.GetButton("Jump") && firstPersonController.inSpaceShip){
            float distanceBetweenRef = Vector3.Distance(transform.position, reference ? reference.transform.position : Vector3.zero);
            
            rb.AddForce(transform.forward * puissance);
            gaz.Play();
        }
        else
        {
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


        
    }

    

    

    private void FixedUpdate()
    {
        CelestialBody[] bodies = Simulation.bodies;
        Vector3 strongestGravitionalPull = Vector3.zero;

        if (Grounded)
        {
            rb.linearVelocity = reference.GetComponent<CelestialBody>().currentVelocity;
        }

        //Gravity
        foreach (CelestialBody body in bodies)
        {
            float sqrtDst = (body.transform.position - rb.position).sqrMagnitude;
            Vector3 forceDir = (body.transform.position - rb.position).normalized;
            Vector3 acceleration = forceDir * constantValues.GravityConstant * body.mass / sqrtDst;
            if (!Grounded)
                rb.AddForce(acceleration * 10f, ForceMode.Acceleration);

            //Find Body with strongest gravitanional pull
            if (acceleration.sqrMagnitude > strongestGravitionalPull.sqrMagnitude)
            {
                strongestGravitionalPull = acceleration;
                reference = body;

            }

            

        }
        

        if (firstPersonController.inSpaceShip){
            
            rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        }

        if (!firstPersonController.inSpaceShip) return;

        // Rotation horizontale (autour de Y)
        float mouseX = Input.GetAxis("Mouse X") * firstPersonController.mouseSensitivityX * Time.fixedDeltaTime;
        Quaternion rotationY = Quaternion.Euler(0f, mouseX, 0f);
        rb.MoveRotation(rb.rotation * rotationY);

        // Rotation verticale (autour de X)
        float mouseY = Input.GetAxis("Mouse Y") * firstPersonController.mouseSensitivityY * Time.fixedDeltaTime;
            if (!Grounded) //si on est pas posé on ne peut pas tourner sur cet axe
        mouseY = Mathf.Clamp(mouseY, -60f, 10f);
        Quaternion rotationX = Quaternion.Euler(-mouseY, 0f, 0f);
        rb.MoveRotation(rb.rotation * rotationX);

    }

    void LateUpdate(){
        if (firstPersonController.inSpaceShip && 1==0)
        {
            Quaternion XRotation = Quaternion.Euler(Vector3.forward * Input.GetAxis("Mouse X") * firstPersonController.mouseSensitivityX * Time.deltaTime);
            rb.MoveRotation(XRotation * rb.rotation);

            Quaternion YRotation = Quaternion.Euler(Vector3.left * Input.GetAxis("Mouse Y") * firstPersonController.mouseSensitivityY * Time.deltaTime);
            rb.MoveRotation(YRotation * rb.rotation);

        }
        
    }

}
