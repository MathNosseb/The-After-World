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

    public Vector3 GetBodyAcceleration(CelestialBody body, Vector3 point, float GravityConstant)
    {
        float sqrDst = (body.GetPosition() - point).sqrMagnitude;
        Vector3 forceDir = (body.GetPosition() - point).normalized;
        Vector3 acceleration = forceDir * GravityConstant * body.mass / sqrDst;
        return acceleration;
    }

    public Vector3 CalculateAcceleration(Vector3 point, float GravityConstant,out CelestialBody strongestGravitationaBody, 
        CelestialBody ignoreBody = null)
    {
        Vector3 acceleration = Vector3.zero;
        strongestGravitationaBody = null;
        foreach (var body in bodies)
        {
            if (body != ignoreBody)
            {
                acceleration += GetBodyAcceleration(body, point, GravityConstant);
               
                //calcul de la force de gravitation du corp
                if (strongestGravitationaBody == null)
                {
                    strongestGravitationaBody = body;
                }
                else
                {
                    if (GetBodyAcceleration(body, point, GravityConstant).sqrMagnitude > 
                        GetBodyAcceleration(strongestGravitationaBody, point, GravityConstant).sqrMagnitude)
                    {
                        strongestGravitationaBody = body;
                    }
                }
                
            }
        }

        return acceleration;
    }

    public bool influenceByBody(Transform self, CelestialBody referenceBody)
    {
        float distance = Vector3.Distance(self.position, referenceBody.transform.position);
        return distance <= referenceBody.distanceBeforeRotation ? true : false;
    }
}
