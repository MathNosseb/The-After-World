using UnityEditor.Rendering;
using UnityEngine;

[RequireComponent(typeof(PlayerContainer))]
public class FPScontroller : MonoBehaviour
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

    private void Awake()
    {
        playerContainer = GetComponent<PlayerContainer>();
        canMove = true;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //detection du sol
        grounded = false;
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.1f) && hit.collider.gameObject != playerContainer.PlayerGO)//detection sol en evitant le joueur
        {
            grounded = true;
            groundRefGameObject = hit.collider.gameObject;
        }
        else
            groundRefGameObject = null;
    }

    private void FixedUpdate()
    {
        //calcul du mouvement
        if (playerContainer.influenceByBody)
        {
            Vector3 playerMove = Vector3.zero;
            if (canMove)
                playerMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
            Vector3 planetMove = playerContainer.reference.currentVelocity * Time.fixedDeltaTime;
            playerContainer.PlayerRB.MovePosition(playerContainer.PlayerRB.position + playerMove + planetMove);
        }
    }

    public void HandleMove(Vector3 moveDirection)
    {
        Vector3 targetMoveAmount = grounded ? moveDirection * playerContainer.WalkSpeed : moveDirection * playerContainer.WalkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount * moveSpeedMultipler, ref smoothMoveVelocity, .15f);
    }

    public void HandleMouse(Vector2 mouse)
    {
        Quaternion axeYRotation = Quaternion.Euler(Vector3.up * mouse.x * Time.deltaTime * playerContainer.Sensibility * rotateSpeedMultiplier);
        playerContainer.PlayerRB.MoveRotation(playerContainer.PlayerRB.rotation * axeYRotation);

        verticalLookRotation += mouse.y * Time.deltaTime * playerContainer.Sensibility * rotateSpeedMultiplier;//direction en Z
        Quaternion axeZRotation = Quaternion.Euler(Vector3.left * mouse.y * Time.deltaTime * playerContainer.Sensibility * rotateSpeedMultiplier);

        if (playerContainer.influenceByBody)
        {
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60f, 60f);//rotation cam (mouvemet planeteraire) -60,60 limite du mouvement
            playerContainer.cameraT.localEulerAngles = Vector3.left * verticalLookRotation;
        }
        else
        {
            playerContainer.cameraT.localRotation = Quaternion.identity;//on fixe la cam sur un axe 0,0,0
            playerContainer.PlayerRB.MoveRotation(playerContainer.PlayerRB.rotation * axeZRotation);
        }
    }

    public void HandleJump()
    {
        playerContainer.PlayerRB.MovePosition(playerContainer.PlayerRB.position + transform.up * 0.1f);//eviter le glitch d etre pris dans le sol
        playerContainer.PlayerRB.AddForce(transform.up * playerContainer.JumpForce); //saut 
    }

}
