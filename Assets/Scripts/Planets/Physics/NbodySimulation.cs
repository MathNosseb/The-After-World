using UnityEngine;

public class NbodySimulation : MonoBehaviour
{
    [HideInInspector]
    public CelestialBody[] bodies;

    void Awake()
    {
        bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].UpdateVelocity(bodies, dt);
        }
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].UpdatePosition(dt);
        }
    }
}
