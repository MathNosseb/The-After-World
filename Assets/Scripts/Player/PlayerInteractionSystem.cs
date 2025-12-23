using UnityEngine;

[RequireComponent(typeof(PlayerContainer))]
public class PlayerInteractionSystem : MonoBehaviour
{

    PlayerContainer playerContainer;

    [Header("Intercation")]
    GameObject interactibleObject;
    public bool canInteract { get; private set; }

    private void Awake()
    {
        playerContainer = GetComponent<PlayerContainer>();
    }
    private void Update()
    {
        canInteract = false;
        //tirer un raycast depuis la cam pour savoir si on peut interagir avec l objet
        Ray ray = new Ray(playerContainer.cameraT.position, playerContainer.cameraT.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10f))
        {
            canInteract = true;
            interactibleObject = hit.collider.gameObject;
        }
        else
            interactibleObject = null;
    }

    public void OnInteract()
    {
        //vérifie si on peut interagir
        Debug.Log("demande d'interaction");
        if (!canInteract) return;
        IInteractable interactable = interactibleObject.GetComponent<IInteractable>();
        if (interactable == null) return; //si l objet possede une interaction
        Debug.Log("can interact");
        interactable.Interact(playerContainer);
    }
}
