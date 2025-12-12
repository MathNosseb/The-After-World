using UnityEngine;

public class dataHolder : MonoBehaviour
{
    [Header("player")]
    public GameObject player;
    public GameObject cam;
    public FirstPersonController firstPersonController;
    public Rigidbody playerRb { get; private set; }
    public float playerVelocity {  get; private set; }
    public bool inSpaceShip { get; private set; }
    [HideInInspector] public CelestialBody playerReference { get; private set; }
    [HideInInspector] public bool spaceMovement { get; private set; }
    [HideInInspector] public GameObject referenceGround { get; private set; }

    [Header("spaceShip")]
    public GameObject spaceShip;
    public SpaceMovementsGravity spaceMovementsGravity;
    public Rigidbody spaceShipRb { get; private set; }
    public float spaceShipVelocity { get; private set; }
    [HideInInspector] public CelestialBody spaceShipReference { get; private set; }

    [Header("Gobal")]
    public SoundsManager soundsManager;

    //params
    float fps;

    private void Start()
    {
        //initialisation joueur
        firstPersonController = player.GetComponent<FirstPersonController>();
        playerRb = player.GetComponent<Rigidbody>();

        //initialisation du vaisseau
        spaceMovementsGravity = spaceShip.GetComponent<SpaceMovementsGravity>();
        spaceShipRb = spaceShip.GetComponent<Rigidbody>();

        //initialisation des sons
        soundsManager = cam.GetComponent<SoundsManager>();


        UpdateParams();
    }

    private void Update()
    {

        UpdateParams();



    }

    /// <summary>
    /// récupère les FPS de la scène
    /// </summary>
    /// <param name="deltaTime">Time.deltaTime</param>
    /// <returns>fps</returns>
    public float GetFps(float deltaTime)
    {
        float rawFps = 1 / deltaTime;
        fps = Mathf.Lerp(fps, rawFps, .1f);

        return fps;
    }

    private void UpdateParams()
    {
        playerVelocity = playerRb.linearVelocity.magnitude;
        inSpaceShip = firstPersonController.inSpaceShip;
        playerReference = firstPersonController.reference;
        spaceMovement = firstPersonController.SpaceMovement;
        referenceGround = firstPersonController.referenceGround;

        spaceShipVelocity = spaceShipRb.linearVelocity.magnitude;
        spaceShipReference = spaceMovementsGravity.reference;
    }
}
