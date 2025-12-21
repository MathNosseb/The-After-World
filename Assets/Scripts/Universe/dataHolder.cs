using UnityEngine;

public class dataHolder : MonoBehaviour
{
    #region player
    [Header("player")]
    public GameObject player;
    public GameObject cam;
    public Rigidbody playerRb;
    public FirstPersonController firstPersonController;
    
    public float playerVelocity {  get; private set; }
    public bool inSpaceShip { get; private set; }
    [HideInInspector] public CelestialBody playerReference { get; private set; }
    [HideInInspector] public bool spaceMovement { get; private set; }
    [HideInInspector] public GameObject referenceGround { get; private set; }

    public float mouseSensitivityX = 250f;
    public float mouseSensitivityY = 250f;
    #endregion

    #region spaceShip
    [Header("spaceShip")]
    public GameObject spaceShip;
    public SpaceMovementsGravity spaceMovementsGravity;
    public Rigidbody spaceShipRb { get; private set; }
    public float spaceShipVelocity { get; private set; }
    [HideInInspector] public CelestialBody spaceShipReference { get; private set; }
    #endregion

    [Header("Gobal")]
    public SoundsManager soundsManager;
    public GameObject Sun;
    public float distanceBeforeFloatingPointRepare;

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


    #region getFPS
    /// <summary>
    /// r�cup�re les FPS de la sc�ne
    /// </summary>
    /// <param name="deltaTime">Time.deltaTime</param>
    /// <returns>fps</returns>
    public float GetFps(float deltaTime)
    {
        float rawFps = 1 / deltaTime;
        fps = Mathf.Lerp(fps, rawFps, .1f);

        return fps;
    }
    #endregion

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

    /// <summary>
    /// Retourne le sujet de la scene, le joueur ou le vaisseau.
    /// </summary>
    /// <returns>le sujet</returns>
    public GameObject GetSubjectGameObject()
    {
        return inSpaceShip ? spaceShip : player;
    }

    public Rigidbody GetSubjectRigidbody()
    {
        return inSpaceShip ? spaceShipRb : playerRb;
    }
}
