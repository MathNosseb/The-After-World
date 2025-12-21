using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(FPScontroller))]
[RequireComponent(typeof(PlayerGravity))]
[RequireComponent(typeof(PlayerUI))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerContainer : MonoBehaviour
{
    [Header("Global References")]
    [SerializeField] private Container GlobalContainer;

    [Header("Player References")]
    FPScontroller FPScontroller;
    PlayerGravity PlayerGravity;
    PlayerUI PlayerUI;
    public List<Notifications> notifications = new List<Notifications>();

    [Header("Player Objects")]
    public Transform cameraT;
    public GameObject PlayerGO { get; private set; }
    public Rigidbody PlayerRB { get; private set; }
    public bool influenceByBody { get; private set; }
    public bool inSpaceShip { get; private set; }
    [HideInInspector] public CelestialBody reference;
    public Vector3 strongestGravitationalPull { get; private set; }
    public GameObject groundRefGameObject { get; private set; }



    [Header("Player Parameters")]
    [SerializeField] private float sensibility = 250f;
    [SerializeField] private float airWalkSpeed = 5f;
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;

    public float Sensibility => sensibility;
    public float AirWalkSpeed => airWalkSpeed;
    public float WalkSpeed => walkSpeed;
    public float JumpForce => jumpForce;


    public void Awake()
    {
        FPScontroller = GetComponent<FPScontroller>();
        PlayerGravity = GetComponent<PlayerGravity>();
        PlayerUI = GetComponent<PlayerUI>();

        PlayerGO = gameObject;
        PlayerRB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        influenceByBody = InfluenceByBody(PlayerGO.transform, reference);
        strongestGravitationalPull = GetBodyAcceleration(reference, PlayerRB.position);
        reference = PlayerGravity.reference;
        groundRefGameObject = FPScontroller.groundRefGameObject;
    }


    private void OnEnable()
    {
        GlobalContainer.inputManager.OnMouseMove += FPScontroller.HandleMouse;
        GlobalContainer.inputManager.OnMove += FPScontroller.HandleMove;
        GlobalContainer.inputManager.OnJump += FPScontroller.HandleJump;
    }

    private void OnDisable()
    {
        GlobalContainer.inputManager.OnMouseMove -= FPScontroller.HandleMouse;
        GlobalContainer.inputManager.OnMove -= FPScontroller.HandleMove;
        GlobalContainer.inputManager.OnJump -= FPScontroller.HandleJump;
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
