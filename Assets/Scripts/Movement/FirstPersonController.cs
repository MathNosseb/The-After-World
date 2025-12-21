using UnityEditor;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Parametres")] //parametres du joueur (mouvements, sensi..)
    public float rotationSpeed = 1.0f;
    public float walkSpeed = 8f;
    public float airWalkSpeed = 1f;
    public float jumpForce = 220;
    public bool affectByGravity = true;
    public bool cinematicMode = false;

    //variables d instances
    Rigidbody rb;
    constant constantValues; 
    PlayerUI playerUI;
    dataHolder data;


    [Header("Camera")] //mouvements de la camera
    public Transform cameraT;
    float verticalLookRotation;
    
    //Mouvements smooth 
    [HideInInspector] public Vector3 moveAmount;
    Vector3 smoothMoveVelocity;


    //references de saut
    bool Grounded;
    [HideInInspector]public GameObject referenceGround;
    bool jumping;


    [Header("contraintes")]
    public bool SpaceMovement = false; 
    bool canMove;


    [Header("vaisseau spatial")]// reference du vaisseau
    public GameObject spaceShip;
    SpaceMovementsGravity spaceMovementsGravity;
    public GameObject playerHolderSpaceShip; //l endroit ou le joueur va etre placé
    public float distanceForEnter; 
    public bool inSpaceShip = false; 
    Notifications notifSpaceShip;

    
    [Header("Gravity")]
    public Vector3 initialVelocity;
    public CelestialBody reference;
    NbodySimulation Simulation;


    bool burning = false;

    

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
        spaceMovementsGravity = spaceShip.GetComponent<SpaceMovementsGravity>();
        data = GameObject.Find("Universe").GetComponent<dataHolder>();

        //set des variables par defaut
        canMove = true;
        if (cinematicMode)
            affectByGravity = false;

        //force initial
        rb.AddForce(initialVelocity, ForceMode.VelocityChange);
    }
    
    private void Update()
    {

        burning = Input.GetButton("Jump");

        //detection entrées sorties du vaisseau
        #region interaction vaisseau

        float distanceWithSpaceShip = Vector3.Distance(spaceShip.transform.position, gameObject.transform.position);
        if (distanceWithSpaceShip < distanceForEnter && notifSpaceShip == null && !inSpaceShip)
            notifSpaceShip = playerUI.SendNotification(500f, 100f, -1f, "press F for enter");
        else if ((distanceWithSpaceShip > distanceForEnter && notifSpaceShip != null) || (inSpaceShip && notifSpaceShip != null))
        {
            playerUI.DestroyNotificationNow(notifSpaceShip);
            notifSpaceShip = null;
        }

        if (Input.GetKeyDown(KeyCode.F) && distanceWithSpaceShip < distanceForEnter)
        {

            if (inSpaceShip)
            {
                //on sort du vaisseau
                //on s'aligne avec la planete
                AllignToPlanet(transform, reference, reference.distanceBeforeRotation, SpaceMovement, GetAcceleration(reference), 1000f);

                //on se met a une position sur le vaisseau
                Vector3 positionOutSpaceShip = spaceShip.transform.position + new Vector3(0f, 5);
                //ajout d une detection pour trouver un meilleur endroit où spawn
                //mais flemme pour le moment
                rb.position = positionOutSpaceShip;

                transform.SetParent(null);

                rb.isKinematic = false;
                rb.detectCollisions = true;


            }

            inSpaceShip = !inSpaceShip;
        }


        if (!inSpaceShip)
        {
            canMove = true;
            rb.isKinematic = false;
        }

        #endregion

        //detection de si on est en mouvement orbital ou planetaire
        #region detection du type de mouvement
        if (influenceByBody(transform, reference))
        {
            SpaceMovement = false;
        }
        else
            SpaceMovement = true;
        #endregion

        if (inSpaceShip)
            return;


        #region calcul smooth mouvement
        //calcul de la direction + si on est dans les air ou sur terre
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 targetMoveAmount =  Grounded ? moveDirection * walkSpeed : moveDirection * airWalkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);
        #endregion

        //raycast pour la detection du sol (taille 1.1f -> taille du joueur depuis le centre 1f + marge .1f)
        #region detection du sol
        //variables de saut
        Grounded = false;
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit,1.1f) && hit.collider.gameObject != gameObject && hit.collider.gameObject != playerHolderSpaceShip)//detection sol en evitant le joueur ou la plateforme
        {

            Grounded = true;
            referenceGround = hit.collider.gameObject;
        }
        else
        {
            referenceGround = null;
        }
        #endregion



        #region handle jump
        if (Input.GetButtonDown("Jump") && Grounded) //saut 
            jumping = true;

        #endregion



    }
    

 
    private void LateUpdate()
    {
        #region gère les mouvements de caméra dans le late pour eviter tout jittering
        if (!inSpaceShip && canMove)// si on est pas dans le vaisseau et que l on peut bouger
            CameraMovement(-60f, 60f);
        if (inSpaceShip)//si on est dans le vaisseau
            EnterSpaceShip();// effectuer ici car influence la position de la caméra
        #endregion
    }


    
    private void FixedUpdate()
    {

        if (cinematicMode || inSpaceShip) //si on est pas en mode cinematique ni dans le vaisseau on execute physique + mouvement + alignement
            return;

        #region gère le saut
        if (jumping) //saut 
        {
            rb.MovePosition(rb.position + transform.up * 0.1f);//eviter le glitch d etre pris dans le sol
            rb.AddForce(transform.up * jumpForce); //saut 
            jumping = false;
        }
        #endregion

        #region attache au sol et mouvement
        if (influenceByBody(transform, reference)){// on se place a la meme vitesse que la planete pour tenir sur elle sans glisser
            Vector3 playerMove = Vector3.zero;
            if (canMove)
                playerMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
            Vector3 planetMove = reference.currentVelocity * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + playerMove + planetMove);
        }
        #endregion

        #region calcul de l'accélération de toutes les planètes + applique les forces
        CelestialBody[] bodies = Simulation.bodies;
        Vector3 strongestGravitionalPull = Vector3.zero;
        foreach (CelestialBody body in bodies)
        {
            //acceleration valeur brut, le sqrMagnitude met au carré sert juste à comparer, donc ici on ignore les force faible sqrt(.1f) = 0.3
            Vector3 acceleration = GetAcceleration(body).sqrMagnitude > .1f ? GetAcceleration(body) : Vector3.zero;//on ignore les forces fable (> 0.3)
            if (affectByGravity)//applique la gravité si on subit la gravité
                rb.AddForce(acceleration, ForceMode.Acceleration);//acceleration car independant de la masse

            reference = GetStrongestBodyAcceleration(body, strongestGravitionalPull);
            strongestGravitionalPull = GetAcceleration(reference);

        }

        #endregion

        //allignement avec la planête
        AllignToPlanet(transform, reference, reference.distanceBeforeRotation, SpaceMovement, strongestGravitionalPull);

        
    }   

    public bool influenceByBody(Transform self, CelestialBody referenceBody)
    {
        float distance = Vector3.Distance(self.position, referenceBody.transform.position);
        return distance <= referenceBody.distanceBeforeRotation ? true : false;
    }


    /// <summary>
    /// fonction qui s'éxecute lorsque l'on entre dans le vaisseai
    /// </summary>   
    void EnterSpaceShip(){ 
        canMove = false;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        bool condition = spaceMovementsGravity.firstPersonController.influenceByBody(transform, reference) && spaceMovementsGravity.burning;
        Vector3 jittering = condition ? Random.insideUnitSphere * reference.jitteringStrength: Vector3.zero;
        transform.SetParent(playerHolderSpaceShip.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        //transform.position = playerHolderSpaceShip.transform.position; //on se met à l arriere du vaisseau
        //transform.rotation = playerHolderSpaceShip.transform.rotation;//meme orientation que le vaisseau
        //cameraT.position = playerHolderSpaceShip.transform.position;
        cameraT.rotation = playerHolderSpaceShip.transform.rotation; //bonne orientation de cam

    } 


    /// <summary>
    /// Bouger la Camera en fonction de l'orientation de la souris pour un système spatial
    /// </summary>
    /// <param name="minLimit">orientation la plus basse possible de la caméra en mouvement planétaire</param>
    /// <param name="maxLimit">orientation la plus haute possible de la caméra en mouvement planétaire</param>
    void CameraMovement(float minLimit, float maxLimit)
    {
        //A CHANGER VERS UN RB POUR BOUGER AVEC LA PHYSIQUE
        //transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);//rotation axe X
        Quaternion axeYRotation = Quaternion.Euler(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * data.mouseSensitivityX * rotationSpeed);
        rb.MoveRotation(rb.rotation * axeYRotation);

        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * data.mouseSensitivityY * rotationSpeed;//direction en Z
        Quaternion axeZRotation = Quaternion.Euler(Vector3.left * Input.GetAxis("Mouse Y") * Time.deltaTime * data.mouseSensitivityY * rotationSpeed);
        if (!SpaceMovement)
        {
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, minLimit, maxLimit);//rotation cam (mouvemet planeteraire)
            cameraT.localEulerAngles = Vector3.left * verticalLookRotation;
        }
        else
        {
            cameraT.localRotation = Quaternion.identity;//on fixe la cam sur un axe 0,0,0
            rb.MoveRotation(rb.rotation * axeZRotation);
        }
    }
    /// <summary> 
    /// retourne l'accélération du gameObject sur un corp
    /// </summary>
    /// <param name="body">Le CelestialBody qui influence le gameObject</param>
    /// <returns>l'acceleration du corp</returns>
    Vector3 GetAcceleration(CelestialBody body)
    {
        float sqrtDst = (body.transform.position - rb.position).sqrMagnitude;
        Vector3 forceDir = (body.transform.position - rb.position).normalized;
        Vector3 acceleration = forceDir * constantValues.GravityConstant * body.mass / sqrtDst;

        return acceleration;
    }

    /// <summary>
    /// <br> appelé pour chaque corp et trouve si le body tire plus fort que le precédent</br>
    /// <br> il est important de l'appeler pour chaque corps dans une autre fonction il compare uniquement</br>
    /// </summary>
    /// <param name="body">Le CelestialBody que l'on souhaite comparer</param>
    /// <param name="strongestGravitionalPull">L'attraction la plus forte</param>
    /// <returns>retourne l'accélération la plus forte, soit le corp soit celui qui était déjà la plus forte</returns>
    CelestialBody GetStrongestBodyAcceleration(CelestialBody body, Vector3 strongestGravitionalPull)
    {
        //Find Body with strongest gravitanional pull
        if (GetAcceleration(body).sqrMagnitude > strongestGravitionalPull.sqrMagnitude)
        {
            strongestGravitionalPull = GetAcceleration(body);
            return body;
            
        }
        return reference;//le corp le plus fort actuellement
    }


    /// <summary>
    /// <br> aligne le joueur avec la planete cible</br>
    /// <br> utilisé pour aligner un objet avec un autre quand sa distance est bonne </br>
    /// </summary>
    /// <param name="self">l'objet à aligner</param>
    /// <param name="reference">l'objet sur lequel on s'aligne</param>
    /// <param name="PLanetDIstanceAttraction">la distance entre les deux objets à laquel on s'aligne</param>
    /// <param name="SpaceMovement">détecte si on est sur une planete ou en mouvement spatial</param>
    /// <param name="strongestGravitionalPull">l'accélération la plus forte permettant un alignement</param>
    void AllignToPlanet(Transform self, CelestialBody reference, float PLanetDIstanceAttraction, bool SpaceMovement, Vector3 strongestGravitionalPull, float rotationSpeed = 10f)
    {
        if (Vector3.Distance(transform.position, reference.transform.position) < reference.distanceBeforeRotation && !SpaceMovement)
        {
            //Rotate for align with gravity up
            Vector3 gravityUp = -strongestGravitionalPull.normalized;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * rb.rotation;
            Quaternion smoothRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRotation);
        }
        
    }


}
