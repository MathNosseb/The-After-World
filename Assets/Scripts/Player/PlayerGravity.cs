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
        //Calcul de la gravité
        CelestialBody strongestBody;
        Vector3 acceleration = playerContainer.GetGravityAcceleration(playerContainer.PlayerRB.position, out strongestBody);
        reference = strongestBody;

        
        if (usePhysic)
        {
            //application de la gravité
            playerContainer.PlayerRB.AddForce(acceleration, ForceMode.Acceleration);
            //alignement avec la planete
            AllignToPlanet(playerContainer.PlayerGO.transform, playerContainer.reference, playerContainer.strongestGravitationalPull);
            //force de l atmosphere  
            float dst = Vector3.Distance(playerContainer.PlayerRB.position, playerContainer.reference.GetVector3Position());
            Vector3 force = AtmosphericEffect(dst, playerContainer.reference.distanceBeforeRotation, playerContainer.reference.density, playerContainer.strongestGravitationalPull);
            playerContainer.PlayerRB.AddForce(force);
        }
            

    }

    Vector3 AtmosphericEffect(float playerHeight, float AtmosphereHeight, float density, Vector3 strongestGravitionalPull)
    {
        float strength = Mathf.Max(0, 1 - (playerHeight / AtmosphereHeight));
        Vector3 atmosphericForce = strongestGravitionalPull.normalized * strength * density;
        return atmosphericForce;
    }

    void AllignToPlanet(Transform self, CelestialBody reference, Vector3 strongestGravitionalPull, float rotationSpeed = 10f)
    {
        if (playerContainer.InfluenceByBody(self, reference))
        {
            //Rotate for align with gravity up
            Vector3 gravityUp = -strongestGravitionalPull.normalized;
            Quaternion targetRotation = Quaternion.FromToRotation(self.transform.up, gravityUp) * playerContainer.PlayerRB.rotation;
            Quaternion smoothRotation = Quaternion.Slerp(
                playerContainer.PlayerRB.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
            playerContainer.PlayerRB.MoveRotation(smoothRotation);
        }

    }
}