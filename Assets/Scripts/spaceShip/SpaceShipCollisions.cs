using UnityEngine;

[RequireComponent(typeof(SpaceShipContainer))]
public class SpaceShipCollisions : MonoBehaviour
{
    SpaceShipContainer spaceShipContainer;

    void Awake()
    {
        spaceShipContainer = GetComponent<SpaceShipContainer>();
    }

    void OnCollisionEnter(Collision collision)
    {

        
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Collision avec " + other.name);
    }
}
