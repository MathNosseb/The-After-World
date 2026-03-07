using UnityEngine;

[ExecuteInEditMode]
public class OrbitDebugDisplay : MonoBehaviour
{

    public int numSteps = 1000;
    public float timeStep = 0.1f;
    public bool usePhysicsTimeStep;

    public bool relativeToBody;
    public CelestialBody centralBody;
    public float width = 100;

    constant constValue;

    void Start()
    {
        if (Application.isPlaying)
        {
            HideOrbits();
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
            DrawOrbits();
    }

    void DrawOrbits()
    {

        
        
        CelestialBody[] bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
        var virtualBodies = new VirtualBody[bodies.Length];
        var drawPoints = new CelestialBody.DoubleVector3[bodies.Length][];
        int referenceFrameIndex = 0;
        CelestialBody.DoubleVector3 referenceBodyInitialPosition = CelestialBody.DoubleVector3.zero;

        // Initialize virtual bodies (don't want to move the actual bodies)
        for (int i = 0; i < virtualBodies.Length; i++)
        {
            
            virtualBodies[i] = new VirtualBody(bodies[i]);
            
            drawPoints[i] = new CelestialBody.DoubleVector3[numSteps];
            if (bodies[i] == centralBody && relativeToBody)
            {
                referenceFrameIndex = i;
                referenceBodyInitialPosition = virtualBodies[i].position;
            }
        }

        // Simulate
        for (int step = 0; step < numSteps; step++)
        {
            CelestialBody.DoubleVector3 referenceBodyPosition = (relativeToBody) ? virtualBodies[referenceFrameIndex].position : CelestialBody.DoubleVector3.zero;
            // Update velocities
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                virtualBodies[i].velocity += CalculateAcceleration(i, virtualBodies) * timeStep;
            }
            // Update positions
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                CelestialBody.DoubleVector3 newPos = virtualBodies[i].position + virtualBodies[i].velocity * timeStep;
                virtualBodies[i].position = newPos;
                if (relativeToBody)
                {
                    var referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                    newPos -= referenceFrameOffset;
                }
                if (relativeToBody && i == referenceFrameIndex)
                {
                    newPos = referenceBodyInitialPosition;
                }
                drawPoints[i][step] = newPos;
            }
        }

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < virtualBodies.Length; bodyIndex++)
        {
            var pathColour = bodies[bodyIndex].colorPath;
            pathColour.a = 1f;
            
            for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++)
            {
                Debug.DrawLine(drawPoints[bodyIndex][i].convert, drawPoints[bodyIndex][i + 1].convert, pathColour);
                
            }

            // Hide renderer
            var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();
            if (lineRenderer)
            {
                lineRenderer.enabled = false;
            }


        }
    }

    CelestialBody.DoubleVector3 CalculateAcceleration(int i, VirtualBody[] virtualBodies)
    {
        CelestialBody.DoubleVector3 acceleration = CelestialBody.DoubleVector3.zero;
        for (int j = 0; j < virtualBodies.Length; j++)
        {
            if (i == j)
            {
                continue;
            }

            CelestialBody.DoubleVector3 forceDir = (virtualBodies[j].position - virtualBodies[i].position).normalized;
            double sqrDst = (virtualBodies[j].position - virtualBodies[i].position).sqrMagnitude;
            acceleration += forceDir * constValue.GravityConstant * virtualBodies[j].mass / sqrDst; //1 = gravity constant
        }
        return acceleration;
    }

    void HideOrbits()
    {
        CelestialBody[] bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
    }

    void OnValidate()
    {
        constValue = gameObject.GetComponent<constant>();
        
    }

    class VirtualBody
    {
        public CelestialBody.DoubleVector3 position;
        public CelestialBody.DoubleVector3 velocity;
        public float mass;

        public VirtualBody(CelestialBody body)
        {
            position = new CelestialBody.DoubleVector3(
                (double)body.transform.position.x,
                (double)body.transform.position.y,
                (double)body.transform.position.z
            );
            velocity = body.initialVelocity;
            mass = body.mass;
        }
    }
}