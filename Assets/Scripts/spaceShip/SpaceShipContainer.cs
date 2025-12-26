using UnityEngine;


[RequireComponent (typeof(SpaceShipController))]
[RequireComponent(typeof(SpaceShipGravity))]
[RequireComponent(typeof(SpaceShipSoundSystem))]
[RequireComponent(typeof(SoundsManager))]
[RequireComponent(typeof(Rigidbody))]
public class SpaceShipContainer : MonoBehaviour
{
    [Header("Global References")]
    [SerializeField] private Container GlobalContainer;

    [Header("SpaceShip References")]
    SpaceShipController spaceShipController;
    SpaceShipGravity spaceShipGravity;
    SpaceShipSoundSystem spaceShipSoundSystem;
    public SoundsManager soundsManager;

    [Header("SpaceShip Objects")]
    public ParticleSystem gaz;
    public GameObject playerHolder;
    public GameObject SpaceShipGO { get; private set; }
    public Rigidbody SpaceShipRB { get; private set; }
    public bool influenceByBody { get; private set; }
    [HideInInspector] public CelestialBody reference;
    public Vector3 strongestGravitationalPull { get; private set; }
    public GameObject groundRefGameObject { get; private set; }

    [Header("SpaceShip Parameters")]
    public bool playerInSpaceShip { get; private set; }
    [SerializeField] private float burnStrength = 800f;
    [SerializeField] private float sensibility = 250f;
    public float BurnStrength => burnStrength;
    public float Sensibility => sensibility;

    [Header("instance")]
    private bool suscribedInputs = false;

    private void Awake()
    {
        spaceShipController = GetComponent<SpaceShipController>();
        spaceShipGravity = GetComponent<SpaceShipGravity>();
        spaceShipSoundSystem = GetComponent<SpaceShipSoundSystem>();
        soundsManager = GetComponent<SoundsManager>();


        SpaceShipGO = gameObject;
        SpaceShipRB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        reference = spaceShipGravity.reference;
        influenceByBody = InfluenceByBody(SpaceShipGO.transform, reference);
        strongestGravitationalPull = GetBodyAcceleration(reference, SpaceShipRB.position);
        groundRefGameObject = spaceShipController.groundRefGameObject;
        playerInSpaceShip = spaceShipController.playerInSpaceShip;


        //on verifie a chaque frame si le input manager est pret
        if (!suscribedInputs && GlobalContainer != null && GlobalContainer.inputManager != null)
        {
            GlobalContainer.inputManager.OnJump += spaceShipController.HandleBurning;
            GlobalContainer.inputManager.OnJump += spaceShipSoundSystem.HandleBurnSound;
            GlobalContainer.inputManager.OnMouseMove += spaceShipController.HandleRotation;
            
            suscribedInputs = true;
        }

        
    }


    private void OnDisable()
    {
        GlobalContainer.inputManager.OnJump -= spaceShipController.HandleBurning;
        GlobalContainer.inputManager.OnJump -= spaceShipSoundSystem.HandleBurnSound;
        GlobalContainer.inputManager.OnMouseMove -= spaceShipController.HandleRotation;
        
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
}
