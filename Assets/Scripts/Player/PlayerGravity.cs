using UnityEngine;

[RequireComponent(typeof(PlayerContainer))]
public class PlayerGravity : MonoBehaviour
{
    PlayerContainer playerContainer;

    public CelestialBody reference { get; private set; }
    bool usePhysic;
    private void Awake()
    {
        playerContainer = GetComponent<PlayerContainer>();
    }

    private void FixedUpdate()
    {
        if (playerContainer.inSpaceShip)
        {
            usePhysic = false;
        }else
            usePhysic = true;
        //Calcul de la gravit�
        CelestialBody strongestBody;
        CelestialBody.DoubleVector3 acceleration = playerContainer.GetGravityAcceleration(playerContainer.currentPosition, out strongestBody);
        reference = strongestBody;

        //application de la gravité
        if (usePhysic)
            playerContainer.currentVelocity += acceleration * Time.fixedDeltaTime;
            

        //alignement avec la planete    
        if (usePhysic)
            AllignToPlanet(playerContainer.PlayerGO.transform, playerContainer.reference, playerContainer.strongestGravitationalPull);

        playerContainer.currentPosition += playerContainer.currentVelocity * Time.fixedDeltaTime;

    }

    void AllignToPlanet(Transform self, CelestialBody reference, CelestialBody.DoubleVector3 strongestGravitionalPull, float rotationSpeed = 10f)
    {
        if (playerContainer.InfluenceByBody(self, reference))
        {
            //Rotate for align with gravity up
            CelestialBody.DoubleVector3 gravityUp = strongestGravitionalPull.normalized.negative;
            Quaternion targetRotation = Quaternion.FromToRotation(self.transform.up, gravityUp.convert) * playerContainer.PlayerRB.rotation;
            Quaternion smoothRotation = Quaternion.Slerp(
                playerContainer.PlayerRB.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
            playerContainer.PlayerRB.MoveRotation(smoothRotation);
        }

    }
}
