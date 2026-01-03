using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpaceShipContainer))]
public class SpaceShipController : MonoBehaviour, IInteractable
{
    [Header("References")]
    SpaceShipContainer spaceShipContainer;

    [Header("Ground Detection")]
    public bool grounded { get; private set; }
    public GameObject groundRefGameObject { get; private set; }

    [Header("Rotation")]
    public float rotationMultipler = 1f;
    [Range(1f, 100f)]
    public int rotationSmoothEffect;
    Quaternion targetRotation = Quaternion.identity;
    Quaternion smoothRot = Quaternion.identity;

    [Header("parametres")]
    public bool playerInSpaceShip { get; private set; }//sert uniquement à etre recuperer par le spaceShipContainer
    bool burning = false;


    private void Awake()
    {
        spaceShipContainer = GetComponent<SpaceShipContainer>();
        
    }

    private void Update()
    {
        //detection du sol
        grounded = false;
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.1f) && hit.collider.gameObject != spaceShipContainer.SpaceShipGO)//detection sol en evitant le vaisseau
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
        if (spaceShipContainer.influenceByBody)
        {
            Vector3 planetMove = spaceShipContainer.reference.currentVelocity * Time.fixedDeltaTime;
            spaceShipContainer.SpaceShipRB.MovePosition(spaceShipContainer.SpaceShipRB.position + planetMove);
        }

        if (spaceShipContainer.playerInSpaceShip && burning)
            spaceShipContainer.SpaceShipRB.AddForce(spaceShipContainer.SpaceShipGO.transform.forward * spaceShipContainer.BurnStrength);

        spaceShipContainer.SpaceShipRB.MoveRotation(smoothRot);

    }

    public void HandleRotation(Vector3 mouse)
    {
        if (spaceShipContainer.playerInSpaceShip)
        {
            float yawInput = mouse.x * spaceShipContainer.Sensibility * rotationMultipler / 100;
            float pitchInput = mouse.y * spaceShipContainer.Sensibility * rotationMultipler / 100;
            float rollInput = mouse.z; //qwerty (50f = sensi)

            //Calculate rotation 
            var yaw = Quaternion.AngleAxis(yawInput, transform.up);
            var pitch = Quaternion.AngleAxis(-pitchInput, transform.right);
            var roll = Quaternion.AngleAxis(-rollInput, transform.forward);

            targetRotation = yaw * pitch * roll * targetRotation;

            smoothRot = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothEffect);//10f smooth rotation Speed
        }
        
    }

    public void HandleBurning(bool burn)
    {
        burning = burn;
        if (burning && spaceShipContainer.playerInSpaceShip)
            spaceShipContainer.gaz.Play();
        else
            spaceShipContainer.gaz.Stop();
    }

    public void Interact(PlayerContainer playerContainer)
    {
        //A changer vers qqchose de plus sécuriser
        playerContainer.inSpaceShip = !playerContainer.inSpaceShip;
        playerInSpaceShip = playerContainer.inSpaceShip;
        if (playerInSpaceShip)
        {
            playerContainer.playerFixedPoint = spaceShipContainer.playerHolder;
            playerContainer.spaceShipRB = spaceShipContainer.SpaceShipRB;
        }
        else
        {
            playerContainer.playerFixedPoint = null;
            playerContainer.spaceShipRB = null;
        }
            
    }
}
