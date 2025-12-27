using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerGravity))]
[RequireComponent(typeof(PlayerInteractionSystem))]
[RequireComponent(typeof(PlayerSpaceShipManager))]
[RequireComponent(typeof(PlayerUI))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
public class PlayerContainer : MonoBehaviour
{
    [Header("Global References")]
    [SerializeField] private Container GlobalContainer;

    [Header("Player References")]
    PlayerController PlayerController;
    PlayerGravity PlayerGravity;
    PlayerInteractionSystem playerInteractionSystem;
    PlayerUI PlayerUI;
    PlayerSpaceShipManager PlayerSpaceShipManager;
    public List<Notifications> notifications = new List<Notifications>();
    public MeshRenderer playerMeshRenderer { get; private set; }
    public Collider playerCollider { get; private set; }

    [Header("Player Objects")]
    public Transform cameraT;
    public GameObject PlayerGO { get; private set; }
    public Rigidbody PlayerRB { get; private set; }
    public bool influenceByBody { get; private set; }
    public bool inSpaceShip;
    private bool lastInSpaceShip;//permet de detecter un changement dans l etat
    [HideInInspector] public CelestialBody reference;
    public Vector3 strongestGravitationalPull { get; private set; }
    public GameObject groundRefGameObject { get; private set; }
    Notifications interactionNotif;
    [HideInInspector] public GameObject playerFixedPoint;
    [HideInInspector] public Rigidbody spaceShipRB;



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

    //events
    public event Action<bool> OnChangementSpaceShip;
    

    public void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
        PlayerGravity = GetComponent<PlayerGravity>();
        playerInteractionSystem = GetComponent<PlayerInteractionSystem>();
        PlayerUI = GetComponent<PlayerUI>();
        PlayerSpaceShipManager = GetComponent<PlayerSpaceShipManager>();
        playerCollider = GetComponent<Collider>();
        playerMeshRenderer = GetComponent<MeshRenderer>();

        PlayerGO = gameObject;
        PlayerRB = GetComponent<Rigidbody>();

        //vérifications des composants
        if (PlayerController == null) Debug.LogError("player FPScontroller = null");
        if (PlayerGravity == null) Debug.LogError("player PlayerGravity = null");
        if (playerInteractionSystem == null) Debug.LogError("player playerInteractionSystem = null");
        if (PlayerUI == null) Debug.LogError("player PlayerUI = null");
        if (playerCollider == null) Debug.LogError("player playerCollider = null");
        if (playerMeshRenderer == null) Debug.LogError("player playerMeshRenderer = null");
        if (PlayerRB == null) Debug.LogError("player PlayerRB = null");

    }

    private void Start()
    {
        lastInSpaceShip = inSpaceShip;
    }

    private void Update()
    {
        reference = PlayerGravity.reference;
        influenceByBody = InfluenceByBody(PlayerGO.transform, reference);
        strongestGravitationalPull = GetBodyAcceleration(reference, PlayerRB.position);
        groundRefGameObject = PlayerController.groundRefGameObject;

        //on verifie a chaque frame si le input manager est pret
        if (!suscribedInputs && GlobalContainer != null && GlobalContainer.inputManager != null)
        {
            GlobalContainer.inputManager.OnMouseMove += PlayerController.HandleMouse;
            GlobalContainer.inputManager.OnMove += PlayerController.HandleMove;
            GlobalContainer.inputManager.OnJump += PlayerController.HandleJump;
            GlobalContainer.inputManager.OnInteract += playerInteractionSystem.OnInteract;
            OnChangementSpaceShip += PlayerSpaceShipManager.HandleChangementSpaceShip;
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


        //gère la transmition de l'information en envoyant un signal lors du changement d'état
        if (inSpaceShip != lastInSpaceShip)
        {
            //changement d etat
            //changement sortie -> entrée
            //changement entrée -> sortie
            OnChangementSpaceShip?.Invoke(inSpaceShip);
            
        }
        lastInSpaceShip = inSpaceShip;

        
        
    }

    private void OnDisable()
    {
        GlobalContainer.inputManager.OnMouseMove -= PlayerController.HandleMouse;
        GlobalContainer.inputManager.OnMove -= PlayerController.HandleMove;
        GlobalContainer.inputManager.OnJump -= PlayerController.HandleJump;
        GlobalContainer.inputManager.OnInteract -= playerInteractionSystem.OnInteract;
        OnChangementSpaceShip -= PlayerSpaceShipManager.HandleChangementSpaceShip;
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

    public Rigidbody GetReferenceRigidbody()
    {
        if (spaceShipRB != null)
            return spaceShipRB;
        return PlayerRB;
    }

}
