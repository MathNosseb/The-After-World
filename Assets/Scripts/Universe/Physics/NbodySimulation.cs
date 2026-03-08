using UnityEngine;

public class NbodySimulation : MonoBehaviour
{
    [HideInInspector]
    public CelestialBody[] bodies;

    void Awake()
    {
        bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID);
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

    public CelestialBody GetBodyByIndex(int index)
    {
        if (index < 0) index = 0;                  // éviter négatif
        if (index >= bodies.Length) index = bodies.Length - 1; // dernier élément valide
        return bodies[index];
    }

    public CelestialBody.DoubleVector3 GetBodyAcceleration(CelestialBody body, CelestialBody.DoubleVector3 point, float GravityConstant)
    {
        double sqrDst = (body.GetDoubleVector3Position() - point).sqrMagnitude;
        CelestialBody.DoubleVector3 forceDir = (body.GetDoubleVector3Position() - point).normalized;
        CelestialBody.DoubleVector3 acceleration = forceDir * GravityConstant * body.mass / sqrDst;
        return acceleration;
    }

    public CelestialBody.DoubleVector3 CalculateAcceleration(CelestialBody.DoubleVector3 point, float GravityConstant,
        out CelestialBody strongestGravitationaBody, 
        CelestialBody ignoreBody = null)
    {
        CelestialBody.DoubleVector3 acceleration = CelestialBody.DoubleVector3.zero;
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
