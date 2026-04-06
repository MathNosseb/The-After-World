using UnityEngine;

[RequireComponent(typeof(PlayerContainer))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    PlayerContainer playerContainer;
    
    [Header("Ground Detection")]
    public bool grounded { get; private set; }
    public GameObject groundRefGameObject { get; private set; }

    [Header("Movements")]
    public float moveSpeedMultipler = 1f;
    private Vector3 moveAmount;
    private Vector3 smoothMoveVelocity;
    private bool canMove;

    [Header("Rotation")]
    public float rotateSpeedMultiplier = 1f;
    private float verticalLookRotation;

    [Header("Parametres")]
    //variable permettant de detecter un changement d etat
    //ex inspaceShip = true -> inspaceShip = false
    bool lastInSpaceShip;
    

    private void Awake()
    {
        playerContainer = GetComponent<PlayerContainer>();
        canMove = true;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        lastInSpaceShip = playerContainer.inSpaceShip;
    }

    private void Update()
    {
        //detection du sol
        grounded = false; 
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.2f) && hit.collider.gameObject != playerContainer.PlayerGO)//detection sol en evitant le joueur
        {
            grounded = true;
            groundRefGameObject = hit.collider.gameObject;
        }
        else
            groundRefGameObject = null;
    }

    private void LateUpdate()
    {
        
    }

    private void FixedUpdate()
    {
        //calcul du mouvement 
        //s execute uniquement si on est influencé par une planete et que on est pas dans le vaisseau
        if (playerContainer.influenceByBody && !playerContainer.inSpaceShip)
        {
            Vector3 playerMove = Vector3.zero;
            if (canMove)
                playerMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
            Vector3 planetMove = playerContainer.reference.currentVelocity.convert * Time.fixedDeltaTime;
            playerContainer.PlayerRB.MovePosition(playerContainer.PlayerRB.position + playerMove + planetMove);
        }
    }

    public void HandleMove(Vector3 moveDirection)
    {
        //s execute uniquement si on est pas dans le vaisseau
        if (playerContainer.inSpaceShip) return;
        Vector3 targetMoveAmount = grounded ? moveDirection * playerContainer.WalkSpeed : moveDirection * playerContainer.WalkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount * moveSpeedMultipler, ref smoothMoveVelocity, .15f);
    }

    public void HandleMouse(Vector3 mouse)
    {
        // s execute uniquement si on est pas dans le vaisseau
        if (playerContainer.inSpaceShip) return;

        float yaw = mouse.x * playerContainer.Sensibility * rotateSpeedMultiplier * Time.deltaTime;
        float pitch = mouse.y * playerContainer.Sensibility * rotateSpeedMultiplier * Time.deltaTime;

        // rotation horizontale du joueur
        Quaternion axeYRotation = Quaternion.Euler(Vector3.up * yaw);
        playerContainer.PlayerRB.MoveRotation(playerContainer.PlayerRB.rotation * axeYRotation);

        // accumulation rotation verticale
        verticalLookRotation += pitch;

        if (playerContainer.influenceByBody)
        {
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60f, 60f);

            // rotation caméra uniquement
            playerContainer.cameraT.localRotation = Quaternion.Euler(-verticalLookRotation, 0f, 0f);
        }
        else
        {
            // caméra fixe
            playerContainer.cameraT.localRotation = Quaternion.identity;

            // rotation verticale du joueur (espace)
            Quaternion axeZRotation = Quaternion.Euler(Vector3.left * pitch);
            playerContainer.PlayerRB.MoveRotation(playerContainer.PlayerRB.rotation * axeZRotation);
        }
    }

    public void HandleJump(bool jumping)
    {
        //s execute uniquement si on est pas dans le vaisseau
        //gère le jump (est executé par un Update)
        if (!grounded || playerContainer.inSpaceShip || !jumping) return;
        playerContainer.PlayerRB.MovePosition(playerContainer.PlayerRB.position + transform.up * 0.1f);//eviter le glitch d etre pris dans le sol
        playerContainer.PlayerRB.AddForce(transform.up * playerContainer.JumpForce); //saut 
    }

    

}