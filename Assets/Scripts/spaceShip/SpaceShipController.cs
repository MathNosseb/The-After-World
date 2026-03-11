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
        targetRotation = transform.rotation;
        smoothRot = transform.rotation;

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
            Vector3 planetMove = spaceShipContainer.reference.currentVelocity.convert * Time.fixedDeltaTime;
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
            float yawInput = mouse.x * spaceShipContainer.Sensibility * rotationMultipler * Time.deltaTime;
            float pitchInput = mouse.y * spaceShipContainer.Sensibility * rotationMultipler * Time.deltaTime;
            float rollInput = mouse.z * spaceShipContainer.Sensibility * rotationMultipler * Time.deltaTime; //qwerty (50f = sensi)

            //Calculate rotation 
            var yaw = Quaternion.AngleAxis(yawInput, transform.up);
            var pitch = Quaternion.AngleAxis(-pitchInput, transform.right);
            var roll = Quaternion.AngleAxis(-rollInput, transform.forward);

            targetRotation = yaw * pitch * roll * targetRotation;

            smoothRot = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothEffect);//10f smooth rotation Speed
        }
        
    }

    GameObject detectOutput()
    {
        GameObject validOutPoint = null;
        //detecter le lieu de sortie adapté
        foreach (var outpoint in spaceShipContainer.OutPoints)
        {
            Debug.Log(outpoint.gameObject.name);
            Vector3 dir = (outpoint.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, outpoint.transform.position);

            Ray rayOut = new Ray(transform.position, (outpoint.transform.position - transform.position).normalized);
            RaycastHit hitOut;
            if (Physics.Raycast(rayOut, out hitOut, distance))
            {
                if (hitOut.collider != null)
                {
                    if (hitOut.collider.gameObject == spaceShipContainer.SpaceShipGO)
                    {
                        // touche seulement le vaisseau → on considère le point comme valide
                        validOutPoint = outpoint;
                        Debug.DrawRay(transform.position, dir * distance, Color.green);
                        break;
                    }
                    else
                    {
                        // touche un autre obstacle → point invalide
                        Debug.DrawRay(transform.position, dir * distance, Color.red);
                        continue;
                    }
                }

            }
            else
            {
                // rien touché → point valide
                validOutPoint = outpoint;
                Debug.DrawRay(transform.position, dir * distance, Color.green);
                break;
            }
        }

        return validOutPoint;
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
        Debug.Log("aaaaaaa");
        //A changer vers qqchose de plus sécuriser
        if (!playerContainer.inSpaceShip)
        {
            playerContainer.playerFixedPoint = spaceShipContainer.playerHolder;
            playerContainer.spaceShipRB = spaceShipContainer.SpaceShipRB;
            playerContainer.inSpaceShip = true;

        }
        else
        {
            GameObject output = detectOutput();
            
            //on verifie qu on peut sortir
            if (output != null)
            {
                playerContainer.spaceShipOutpoint = output;
                playerContainer.playerFixedPoint = null;
                playerContainer.spaceShipRB = null;
                playerContainer.inSpaceShip = false;
            }
            //on peut pas sortir -> aucune sortie valide
            Debug.Log("aucune sortie valide");
        }
        playerInSpaceShip = playerContainer.inSpaceShip;
            
    }
}