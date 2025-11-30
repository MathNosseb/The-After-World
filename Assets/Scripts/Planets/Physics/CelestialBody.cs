using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    GameObject sun;
    public float surfaceGravity;
    public float radius;
    public Vector3 initialVelocity;
    public bool fix = false;
    public Color colorPath;
    public float spaceDistance;

    [HideInInspector] public Vector3 currentVelocity;
    private Rigidbody rb;

    public bool useAtmosphere = false;
    public AtmosphereGenerator planetAtmosphere;

    constant constantValue;
    Vector3 startPosition;
    [HideInInspector]
    public float mass;

    private void OnValidate()
    {
        constantValue = GameObject.Find("Universe").GetComponent<constant>();
        sun = GameObject.Find("Sun");
        
        mass = surfaceGravity * radius * radius / constantValue.GravityConstant;
        
        
    }


    // Correction de l'attribut [MenuItem] : il doit être appliqué à une méthode statique.
    // Ajout d'une méthode statique pour l'atmosphère.

    [ContextMenu("Mettre à jour la position")]
    public void AtmosphereUpdatePosition()
    {
        if (useAtmosphere) { planetAtmosphere.planetCentre = transform.position; }
    }

    void Awake()
    {
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

            // Évite toute division par zéro
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
}
