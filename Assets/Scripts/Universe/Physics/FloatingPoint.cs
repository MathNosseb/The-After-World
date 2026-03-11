using System;
using UnityEngine;

public class FloatingPoint : MonoBehaviour
{
    CelestialBody[] bodies;
    CelestialBody.DoubleVector3[] positions;
    CelestialBody.DoubleVector3[] velocities;
    public GameObject[] StaticObjects;
    Vector3[] StaticObjectsPositions;

    Vector3 spaceShipPosition;
    Vector3 spaceShipVelocity;

    public GameObject player;
    public GameObject spaceShip;
    Vector3 playerPosition;
    Vector3 playerVelocity;
    public float distanceBeforeCentring;
    public float distance;

    Rigidbody Rb;
    
    PlayerContainer playerContainer;

    private void Awake()
    {
        bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
        positions = new CelestialBody.DoubleVector3[bodies.Length];
        velocities = new CelestialBody.DoubleVector3[bodies.Length];
        playerContainer = player.GetComponent<PlayerContainer>();
        StaticObjectsPositions = new Vector3[StaticObjects.Length];
        
    }

    private void FixedUpdate()
    {
        //quand on arrive a une distance trop loingtaine
        distance = player.transform.position.magnitude;

        //si on depasse la limite
        if (distance < distanceBeforeCentring) { return; }
        Rb = playerContainer.GetReferenceRigidbody();
        Debug.Log("Recentrage de l univers " + Rb);

        //sauvegarde des paramètres 

        for (int bodiIndex = 0; bodiIndex < bodies.Length; bodiIndex++)
        {
            //assignation vitesses et positions des corps
            positions[bodiIndex] = bodies[bodiIndex].GetDoubleVector3Position();
            velocities[bodiIndex] = bodies[bodiIndex].GetDoubleVector3Velocity();
        }

        for (int staticObjectIndex = 0; staticObjectIndex < StaticObjects.Length; staticObjectIndex++)
        {
            StaticObjectsPositions[staticObjectIndex] = StaticObjects[staticObjectIndex].transform.position;
        }

        playerPosition = Rb.position;
        playerVelocity = Rb.linearVelocity;


        Rigidbody spaceShipRb = spaceShip.GetComponent<Rigidbody>();
        bool isKinematicSpaceShip = spaceShipRb.isKinematic;
        if (!playerContainer.inSpaceShip)
        {
            spaceShipPosition = spaceShipRb.position;
            spaceShipVelocity = spaceShipRb.linearVelocity;
            spaceShipRb.isKinematic = true;
        }
        

        bool isKinematic = Rb.isKinematic;

        if (!isKinematic) { Rb.isKinematic = true; }
        

        CelestialBody.DoubleVector3 playerCoordinate = new CelestialBody.DoubleVector3(
            playerPosition.x,
            playerPosition.y,
            playerPosition.z 
        );

        //on deplace chaque objets
        for (int bodiIndex = 0; bodiIndex < bodies.Length; bodiIndex++)
        {
            bodies[bodiIndex].ChangePosition(positions[bodiIndex] - playerCoordinate);
            bodies[bodiIndex].currentVelocity = velocities[bodiIndex];
        }

        for (int staticObjectIndex = 0; staticObjectIndex < StaticObjects.Length; staticObjectIndex++)
        {
            StaticObjects[staticObjectIndex].transform.position = (StaticObjectsPositions[staticObjectIndex] - playerPosition);
        }

        if (!playerContainer.inSpaceShip)
        {
            spaceShipRb.position = spaceShipPosition - playerPosition;
            spaceShipRb.isKinematic = isKinematicSpaceShip;
            spaceShipRb.linearVelocity = spaceShipVelocity;
        }

        //on replace le joueur au centre ou le vaisseau si on est dans le vaisseau
        Rb.position = Vector3.zero;

        //on retabli les paramètres
        
        Rb.isKinematic = isKinematic;
        Rb.linearVelocity = playerVelocity; 

        Debug.Log("fin du décalage");
    }

}