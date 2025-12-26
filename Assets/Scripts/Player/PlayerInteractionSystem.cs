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
        interactibleObject = null;

        Ray ray = new Ray(playerContainer.cameraT.position, playerContainer.cameraT.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, 10f))
            return;

        if (!hit.collider.TryGetComponent<IInteractable>(out _))
            return;

        interactibleObject = hit.collider.gameObject;
        canInteract = true;
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
