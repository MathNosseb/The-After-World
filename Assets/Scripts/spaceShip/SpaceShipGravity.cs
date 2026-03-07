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
        //calcul de la gravit�
        CelestialBody strongestbody;
        CelestialBody.DoubleVector3 acceleration = spaceShipContainer.GetGravityAcceleration(spaceShipContainer.currentPosition, 
            out strongestbody);
        reference = strongestbody;

        //application de la gravité
        spaceShipContainer.SpaceShipRB.AddForce(acceleration.convert, ForceMode.Acceleration);

        //alignement avec la planete
        AllignToPlanet(spaceShipContainer.SpaceShipGO.transform, 
            spaceShipContainer.strongestGravitationalPull);



    }

    void AllignToPlanet(Transform self, CelestialBody.DoubleVector3 strongestGravitionalPull, float rotationSpeed = 10f)
    {
        if (spaceShipContainer.influenceByBody)
        {
            //Rotate for align with gravity up
            CelestialBody.DoubleVector3 gravityUp = strongestGravitionalPull.normalized.negative;
            Quaternion targetRotation = Quaternion.FromToRotation(self.transform.forward, gravityUp.convert) * spaceShipContainer.SpaceShipRB.rotation;
            Quaternion smoothRotation = Quaternion.Slerp(
                spaceShipContainer.SpaceShipRB.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);
            //spaceShipContainer.SpaceShipRB.MoveRotation(smoothRotation);
        }

    }
}
