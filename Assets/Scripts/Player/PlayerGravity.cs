using UnityEngine;

[RequireComponent(typeof(PlayerContainer))]
public class PlayerGravity : MonoBehaviour
{
    PlayerContainer playerContainer;

    public CelestialBody reference { get; private set; }
    private void Awake()
    {
        playerContainer = GetComponent<PlayerContainer>();
    }

    private void FixedUpdate()
    {
        //Calcul de la gravité
        CelestialBody strongestBody;
        Vector3 acceleration = playerContainer.GetGravityAcceleration(playerContainer.PlayerRB.position, out strongestBody);
        reference = strongestBody;

        //application de la gravité
        playerContainer.PlayerRB.AddForce(acceleration, ForceMode.Acceleration);

        //alignement avec la planete
        AllignToPlanet(playerContainer.PlayerGO.transform, playerContainer.reference, playerContainer.strongestGravitationalPull);

    }

    void AllignToPlanet(Transform self, CelestialBody reference, Vector3 strongestGravitionalPull, float rotationSpeed = 10f)
    {
        if (playerContainer.InfluenceByBody(self, reference))
        {
            //Rotate for align with gravity up
            Vector3 gravityUp = -strongestGravitionalPull.normalized;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * playerContainer.PlayerRB.rotation;
            Quaternion smoothRotation = Quaternion.Slerp(
                playerContainer.PlayerRB.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
            playerContainer.PlayerRB.MoveRotation(smoothRotation);
        }

    }
}
