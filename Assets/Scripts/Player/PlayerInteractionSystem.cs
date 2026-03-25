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

        if (playerContainer.inSpaceShip)
        {
            // Quand on est dans le vaisseau → on considère qu'on peut TOUJOURS interagir
            // (pour sortir, peu importe où on regarde)
            canInteract = true;
            // Option A : garder l'ancien interactible (si déjà dedans)
            if (interactibleObject == null)
            {
                // Option B : forcer sur le vaisseau parent le plus proche
                if (Physics.Raycast(playerContainer.cameraT.position, playerContainer.cameraT.forward, out RaycastHit hit, 10f))
                {
                    interactibleObject = hit.collider.gameObject;
                }
                else
                {
                    // Fallback : cherche le vaisseau via le playerHolder
                    interactibleObject = playerContainer.playerFixedPoint?.GetComponentInParent<SpaceShipContainer>()?.gameObject;
                }
            }
            return;
        }

        // Mode normal (hors vaisseau)
        Ray ray = new Ray(playerContainer.cameraT.position, playerContainer.cameraT.forward);
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, 10f))
            return;

        if (!hitInfo.collider.TryGetComponent<IInteractable>(out _))
            return;

        interactibleObject = hitInfo.collider.gameObject;
        canInteract = true;
    }

    public void OnInteract()
    {
        //vérifie si on peut interagir
        Debug.Log("demande d'interaction " + canInteract);
        if (!canInteract) return;
        IInteractable interactable = interactibleObject.GetComponent<IInteractable>();
        if (interactable == null) return; //si l objet possede une interaction
        Debug.Log("can interact");
        interactable.Interact(playerContainer);
    }
}
