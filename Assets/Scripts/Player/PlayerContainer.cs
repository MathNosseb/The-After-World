using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(FPScontroller))]
[RequireComponent(typeof(PlayerGravity))]
[RequireComponent(typeof(PlayerInteractionSystem))]
[RequireComponent(typeof(PlayerUI))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerContainer : MonoBehaviour
{
    [Header("Global References")]
    [SerializeField] private Container GlobalContainer;

    [Header("Player References")]
    FPScontroller FPScontroller;
    PlayerGravity PlayerGravity;
    PlayerInteractionSystem playerInteractionSystem;
    PlayerUI PlayerUI;
    public List<Notifications> notifications = new List<Notifications>();

    [Header("Player Objects")]
    public Transform cameraT;
    public GameObject PlayerGO { get; private set; }
    public Rigidbody PlayerRB { get; private set; }
    public bool influenceByBody { get; private set; }
    public bool inSpaceShip;
    [HideInInspector] public CelestialBody reference;
    public Vector3 strongestGravitationalPull { get; private set; }
    public GameObject groundRefGameObject { get; private set; }
    Notifications interactionNotif;
    public GameObject playerFixedPoint;



    [Header("Player Parameters")]
    [SerializeField] private float sensibility = 250f;
    [SerializeField] private float airWalkSpeed = 5f;
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;

    public float Sensibility => sensibility;
    public float AirWalkSpeed => airWalkSpeed;
    public float WalkSpeed => walkSpeed;
    public float JumpForce => jumpForce;

    [Header("instance")]
    private bool suscribedInputs = false;


    public void Awake()
    {
        FPScontroller = GetComponent<FPScontroller>();
        PlayerGravity = GetComponent<PlayerGravity>();
        playerInteractionSystem = GetComponent<PlayerInteractionSystem>();
        PlayerUI = GetComponent<PlayerUI>();

        PlayerGO = gameObject;
        PlayerRB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        reference = PlayerGravity.reference;
        influenceByBody = InfluenceByBody(PlayerGO.transform, reference);
        strongestGravitationalPull = GetBodyAcceleration(reference, PlayerRB.position);
        groundRefGameObject = FPScontroller.groundRefGameObject;

        //on verifie a chaque frame si le input manager est pret
        if (!suscribedInputs && GlobalContainer != null && GlobalContainer.inputManager != null)
        {
            GlobalContainer.inputManager.OnMouseMove += FPScontroller.HandleMouse;
            GlobalContainer.inputManager.OnMove += FPScontroller.HandleMove;
            GlobalContainer.inputManager.OnJump += FPScontroller.HandleJump;
            GlobalContainer.inputManager.OnInteract += playerInteractionSystem.OnInteract;
            suscribedInputs = true;
        }

        //verification si interaction avec objet possible, si on a pas deja la notif et si on est pas dans le vaisseau
        if (playerInteractionSystem.canInteract && interactionNotif == null && !inSpaceShip)
        {
            //on envoie la notif
            interactionNotif = PlayerUI.SendNotification(500f, 200f, -1f, "press F for interact");
        }else if ((!playerInteractionSystem.canInteract && interactionNotif != null) || (inSpaceShip && interactionNotif != null))
        {
            //si on eput pas interagir mais que on a la norif ou que on est dans le vaisseau
            //on detruit la notif d interaction
            PlayerUI.DestroyNotificationNow(interactionNotif);
            interactionNotif = null;
        }
    }

    private void OnDisable()
    {
        GlobalContainer.inputManager.OnMouseMove -= FPScontroller.HandleMouse;
        GlobalContainer.inputManager.OnMove -= FPScontroller.HandleMove;
        GlobalContainer.inputManager.OnJump -= FPScontroller.HandleJump;
        GlobalContainer.inputManager.OnInteract -= playerInteractionSystem.OnInteract;
    }

    public Vector3 GetGravityAcceleration(Vector3 point, out CelestialBody strongestGravitationalBody, CelestialBody ignoreBody = null)
    {
        //A Changer vers un Action ou un FUNC pour eviter d appeler l appel d une fonction
        return GlobalContainer.GetGravityAcceleration(point, out strongestGravitationalBody, ignoreBody);
    }

    public Vector3 GetBodyAcceleration(CelestialBody body, Vector3 point)
    {
        return GlobalContainer.GetBodyAcceleration(body, point);
    }

    public bool InfluenceByBody(Transform self, CelestialBody referenceBody)
    {
        return GlobalContainer.influenceByBody(self, referenceBody);
    }

    public float GetFps(float deltaTime)
    {
        return GlobalContainer.GetFps(deltaTime);
    }

}
