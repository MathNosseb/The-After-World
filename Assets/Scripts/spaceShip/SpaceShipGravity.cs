using UnityEngine;

[RequireComponent (typeof(SpaceShipContainer))]
public class SpaceShipGravity : MonoBehaviour
{
    SpaceShipContainer spaceShipContainer;
    public CelestialBody reference { get; private set; }


    private void Awake()
    {
        spaceShipContainer = GetComponent<SpaceShipContainer>();
    }

    private void FixedUpdate()
    {
        //calcul de la gravité
        CelestialBody strongestbody;
        Vector3 acceleration = spaceShipContainer.GetGravityAcceleration(spaceShipContainer.SpaceShipRB.position, 
            out strongestbody);
        reference = strongestbody;

        //application de la gravité
        spaceShipContainer.SpaceShipRB.AddForce(acceleration, ForceMode.Acceleration);

        //alignement avec la planete
        AllignToPlanet(spaceShipContainer.SpaceShipGO.transform, 
            spaceShipContainer.strongestGravitationalPull);



    }

    void AllignToPlanet(Transform self, Vector3 strongestGravitionalPull, float rotationSpeed = 10f)
    {
        if (spaceShipContainer.influenceByBody)
        {
            Debug.Log("alignement sur la planete");
            //Rotate for align with gravity up
            Vector3 gravityUp = -strongestGravitionalPull.normalized;
            Quaternion targetRotation = Quaternion.FromToRotation(self.transform.forward, gravityUp) * spaceShipContainer.SpaceShipRB.rotation;
            Quaternion smoothRotation = Quaternion.Slerp(
                spaceShipContainer.SpaceShipRB.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
            spaceShipContainer.SpaceShipRB.MoveRotation(smoothRotation);
        }

    }
}
