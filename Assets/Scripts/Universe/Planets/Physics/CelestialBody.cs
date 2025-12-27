using UnityEditor;
using UnityEngine;
using System.IO;

public class CelestialBody : MonoBehaviour
{
    GameObject sun;
    public float surfaceGravity;
    public float radius;
    public Vector3 initialVelocity;
    public bool fix = false;
    public Color colorPath;

    [HideInInspector] public Vector3 currentVelocity;
    private Rigidbody rb;

    public bool useAtmosphere = false;
    public AtmosphereGenerator planetAtmosphere;

    constant constantValue;
    Vector3 startPosition;
    [HideInInspector]
    public float mass;

    public float distanceBeforeRotation;
    public float jitteringStrength;

    #if UNITY_EDITOR

    private void OnValidate()
    {
        constantValue = GameObject.Find("Universe").GetComponent<constant>();
        sun = GameObject.Find("Sun");
        
        mass = surfaceGravity * radius * radius / constantValue.GravityConstant;
        
        
    }
    #endif


    [ContextMenu("Mettre � jour la position")]
    public void AtmosphereUpdatePosition()
    {
        if (useAtmosphere) { planetAtmosphere.planetCentre = transform.position; }
    }


    void Awake()
    {
        constantValue = GameObject.Find("Universe").GetComponent<constant>();
        sun = GameObject.Find("Sun");
        mass = surfaceGravity * radius * radius / constantValue.GravityConstant;
        currentVelocity = initialVelocity;
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        

    }

    private void FixedUpdate()
    {
        if (fix) { transform.position = startPosition; }
        if (useAtmosphere) { planetAtmosphere.planetCentre = rb.position; }
       
    }

    private void Update()
    { 
        //modifie la light
        if (useAtmosphere) { planetAtmosphere.lightDir = (sun.transform.position - transform.position).normalized; }
    }

    public void UpdateVelocity(CelestialBody[] allBodies, float timeStep)
    {
        if (fix) { return; }
        foreach (var otherBody in allBodies)
        {
            if (otherBody == this) continue;

            Vector3 direction = otherBody.rb.position - rb.position;
            float distanceSqr = direction.sqrMagnitude;

            // �vite toute division par z�ro
            if (distanceSqr < 1e-6f)
                continue;

            Vector3 forceDir = direction.normalized;
            Vector3 acceleration = forceDir * (constantValue.GravityConstant * otherBody.mass / distanceSqr);
            currentVelocity += acceleration * timeStep;
        }
    }

    public void UpdatePosition(float timeStep)
    {
        if (fix) { return; }
        rb.position += currentVelocity * timeStep;
    }

    public void ChangePosition(Vector3 newPosition)
    {
        startPosition = newPosition;
        rb.position = newPosition;
    }

    public Vector3 GetPosition()
    {
        return rb.position;
    }
}
