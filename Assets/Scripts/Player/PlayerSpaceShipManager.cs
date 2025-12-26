using UnityEngine;


[RequireComponent (typeof(PlayerContainer))]
public class PlayerSpaceShipManager : MonoBehaviour
{
    [Header("Réferences")]
    PlayerContainer playerContainer;
    //va gérer le mouvement du joueur lorsqu'il est dans le vaisseau

    private void Awake()
    {
        playerContainer = GetComponent<PlayerContainer>();
    }

    private void Update()
    {
        if (playerContainer.inSpaceShip)
        {
            //si on est dans le vaisseau
            playerContainer.PlayerRB.position = playerContainer.playerFixedPoint.transform.position;
            playerContainer.PlayerRB.rotation = playerContainer.playerFixedPoint.transform.rotation;
            

        }
    }

    private void LateUpdate()
    {
        //la gestion de la camera doit se faire UNIQUEMENT dans le lateUpdate
        if (playerContainer.inSpaceShip)
        {
            playerContainer.cameraT.rotation = playerContainer.playerFixedPoint.transform.rotation;
            playerContainer.cameraT.position = playerContainer.playerFixedPoint.transform.position;
        }
    }

    public void HandleChangementSpaceShip(bool newSpaceShipEtat)
    {
        Debug.Log("changement");
        if (newSpaceShipEtat)
        {
            Debug.Log("entrée dans le vaisseau");
            playerContainer.PlayerRB.isKinematic = false;
        }
        else
        {
            Debug.Log("sortie du vaisseau");
            playerContainer.PlayerRB.isKinematic = true;
            playerContainer.PlayerRB.position = playerContainer.cameraT.position;

            //replacement de la camera sur le joueur et reset de la camera
            playerContainer.cameraT.localPosition = new Vector3(0f,0.5f,0f);
            playerContainer.cameraT.localRotation = Quaternion.identity;
        }
    }

}
