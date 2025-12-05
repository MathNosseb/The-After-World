using UnityEditor;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Parametres")] //parametres du joueur (mouvements, sensi..)
    public float mouseSensitivityX = 250f;
    public float mouseSensitivityY = 250f;
    public float walkSpeed = 8f;
    public float airWalkSpeed = 1f;
    public float jumpForce = 220;
    public bool affectByGravity = true;
    public bool cinematicMode = false;

    //variables d instances
    Rigidbody rb;
    constant constantValues;
    PlayerUI playerUI;


    [Header("Camera")] //mouvements de la camera
    public Transform cameraT;
    float verticalLookRotation;
    
    //Mouvements smooth 
    [HideInInspector] public Vector3 moveAmount;
    Vector3 smoothMoveVelocity;


    //references de saut
    bool Grounded;
    [HideInInspector]public GameObject referenceGround;

    //mouvements
    [HideInInspector] public bool SpaceMovement = false;
    bool canMove;


    [Header("vaisseau spatial")]// reference du vaisseau
    public GameObject spaceShip;
    public GameObject playerHolderSpaceShip; //l endroit ou le joueur va etre placé
    public float distanceForEnter; 
    [HideInInspector] public bool inSpaceShip = false; 
    Notifications notifSpaceShip;

    [Header("Gravity")]
    public Vector3 initialVelocity;
    public CelestialBody reference;
    NbodySimulation Simulation;

    

    private void Start()
    {
        //desactivation de la souris
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //init des variables d instances
        rb = GetComponent<Rigidbody>();
        playerUI = GetComponent<PlayerUI>();    
        Simulation = GameObject.Find("Universe").GetComponent<NbodySimulation>();
        constantValues = GameObject.Find("Universe").GetComponent<constant>();

        //set des variables par defaut
        canMove = true;
        if (cinematicMode)
            affectByGravity = false;

        //force initial
        rb.AddForce(initialVelocity, ForceMode.VelocityChange);
    }
    
    private void Update()
    {
        //calcul de la direction + si on est dans les air ou sur terre
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 targetMoveAmount =  Grounded ? moveDirection * walkSpeed : moveDirection * airWalkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

        if (Input.GetButtonDown("Jump")) //saut 
        {
            if (Grounded)
            {
                transform.position += transform.up * 0.1f; //eviter le glitch d etre pris dans le sol
                rb.AddForce(transform.up * jumpForce); //saut
            }
            
        }

        //variables de saut
        Grounded = false;
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        //detection du sol
        if (Physics.Raycast(ray, out hit,1.1f))
        {
            Grounded = true;
            referenceGround = hit.collider.gameObject;
        }
        else
        {
            referenceGround = null;
        }

        //detection de si on est en mouvement orbital ou planetaire
        if (Vector3.Distance(transform.position, reference.transform.position) <= reference.distanceBeforeRotation)
        {
            SpaceMovement = false;
        }else
            SpaceMovement = true;  


        //entré et sortie dans le vaisseau 

        float distanceWithSpaceShip = Vector3.Distance(spaceShip.transform.position, gameObject.transform.position);
        if (distanceWithSpaceShip < distanceForEnter && notifSpaceShip == null && !inSpaceShip)
            notifSpaceShip = playerUI.SendNotification(500f, 100f, -1f, "press F for enter");
        else if ((distanceWithSpaceShip > distanceForEnter && notifSpaceShip != null )|| (inSpaceShip && notifSpaceShip != null))
        {
            playerUI.DestroyNotificationNow(notifSpaceShip);
            notifSpaceShip = null;
        }
            

        if (Input.GetKeyDown(KeyCode.F) && distanceWithSpaceShip < distanceForEnter)
            inSpaceShip = !inSpaceShip;
        if (!inSpaceShip)
            canMove = true;

    }
    
    void EnterSpaceShip(){ //fonction qui s execute quand on est dans le vaisseau
        canMove = false;
        transform.position = playerHolderSpaceShip.transform.position; //on se met à l arriere du vaisseau
        transform.rotation = playerHolderSpaceShip.transform.rotation;//meme orientation que le vaisseau
        cameraT.rotation = playerHolderSpaceShip.transform.rotation; //bonne orientation de cam

    }


    void CameraMovement(float minLimit, float maxLimit)
    {
        //fonction pour bouger la camera sur son axe ou le joueur en fonction de si on est dans l espace ou posé sur une planete
        //min limit et max limit sont les rotations maximal de la cam sur l axe Z
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);//rotation axe X

        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;//direction en Z
        if (!SpaceMovement)
        {
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, minLimit, maxLimit);//rotation cam (mouvemet planeteraire)
            cameraT.localEulerAngles = Vector3.left * verticalLookRotation;
        }
        else
        {
            cameraT.localRotation = Quaternion.identity;//on fixe la cam sur un axe 0,0,0
            transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY);//on bouge le joueur sur l axe Z
        }
    }


    private void LateUpdate()
    {
        
        if (!inSpaceShip && canMove)// si on est pas dans le vaisseau et que l on peut bouger
            CameraMovement(-60f, 60f);
        if (inSpaceShip)//si on est dans le vaisseau
            EnterSpaceShip();
        
        
    }

    Vector3 GetAcceleration(CelestialBody body)
    {
        float sqrtDst = (body.transform.position - rb.position).sqrMagnitude;
        Vector3 forceDir = (body.transform.position - rb.position).normalized;
        Vector3 acceleration = forceDir * constantValues.GravityConstant * body.mass / sqrtDst;

        return acceleration;
    }

    CelestialBody GetStrongestBodyAcceleration(CelestialBody body, Vector3 strongestGravitionalPull)
    {
        //Find Body with strongest gravitanional pull
        if (GetAcceleration(body).sqrMagnitude > strongestGravitionalPull.sqrMagnitude)
        {
            strongestGravitionalPull = GetAcceleration(body);
            return body;
            
        }
        return reference;
    }


    //s aligner a la planete cible
    void AllignToPlanet(Transform self, CelestialBody reference, float PLanetDIstanceAttraction, bool SpaceMovement, Vector3 strongestGravitionalPull)
    {
        if (Vector3.Distance(transform.position, reference.transform.position) < reference.distanceBeforeRotation && !SpaceMovement)
        {
            //Rotate for align with gravity up
            Vector3 gravityUp = -strongestGravitionalPull.normalized;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * rb.rotation;
            Quaternion smoothRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                10 * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRotation);
        }
        
    }


    private void FixedUpdate()
    {
        if (cinematicMode) //si on est pas en mode cinematique on execute physique + mouvement + alignement
            return;
        CelestialBody[] bodies = Simulation.bodies;
        Vector3 strongestGravitionalPull = Vector3.zero;

        if (!inSpaceShip && canMove)// si on est pas dans l espace + si on peut bouger on effectur le mouvement
            rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);

        if (referenceGround)// on se place a la meme vitesse que la planete pour tenir sur elle sans glisser
            rb.linearVelocity = reference.GetComponent<CelestialBody>().currentVelocity;

        //Gravity
        foreach (CelestialBody body in bodies)
        {
            Vector3 acceleration = GetAcceleration(body).sqrMagnitude > .1f ? GetAcceleration(body) : Vector3.zero;
            if (affectByGravity)
                rb.AddForce(acceleration, ForceMode.Acceleration);

            reference = GetStrongestBodyAcceleration(body, strongestGravitionalPull);
            if (GetAcceleration(body).sqrMagnitude > strongestGravitionalPull.sqrMagnitude)
            {
                strongestGravitionalPull = GetAcceleration(body);
            }

        }

        //Allign
        AllignToPlanet(transform, reference, 30f, SpaceMovement, strongestGravitionalPull);

        
    }   

}
