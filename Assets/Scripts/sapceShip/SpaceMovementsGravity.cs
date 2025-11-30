using Unity.VisualScripting;
using UnityEngine;

public class SpaceMovementsGravity : MonoBehaviour
{
    NbodySimulation Simulation;
    Rigidbody rb;
    constant constantValues;
    [HideInInspector] public CelestialBody reference;
    FirstPersonController firstPersonController;

    [HideInInspector] public Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    GameObject player;
    GameObject referenceGround;
    bool Grounded;
    [HideInInspector] public ParticleSystem gaz;


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
            
            rb.AddForce(transform.up * 500f);
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


        rb.linearDamping = 0;
        
        //Rotate for align with gravity up
        //if (Vector3.Distance(gameObject.transform.position, reference.transform.position) < 40 ){
        if (!firstPersonController.inSpaceShip){
            Vector3 gravityUp = -strongestGravitionalPull.normalized;
            rb.rotation = Quaternion.FromToRotation(transform.up, gravityUp) * rb.rotation;
        }
        
        

        if (firstPersonController.inSpaceShip){
            
            rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        }

    }

    void LateUpdate(){
        if (firstPersonController.inSpaceShip)
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * firstPersonController.mouseSensitivityX);
            transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y") * Time.deltaTime * firstPersonController.mouseSensitivityY);

        }
        
    }

}
