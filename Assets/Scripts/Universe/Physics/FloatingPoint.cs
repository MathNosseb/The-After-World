using System;
using UnityEngine;

public class FloatingPoint : MonoBehaviour
{
    CelestialBody[] bodies;
    CelestialBody.DoubleVector3[] positions;
    CelestialBody.DoubleVector3[] velocities;

    public GameObject player;
    Vector3 playerPosition;
    Vector3 playerVelocity;
    public float distanceBeforeCentring;
    public float distance;

    Rigidbody playerRB;

    private void Awake()
    {
        bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
        positions = new CelestialBody.DoubleVector3[bodies.Length];
        velocities = new CelestialBody.DoubleVector3[bodies.Length];
        playerRB = player.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //quand on arrive a une distance trop loingtaine
        distance = player.transform.position.magnitude;

        //si on depasse la limite
        if (distance < distanceBeforeCentring) { return; }

        Debug.Log("Recentrage de l univers");

        //sauvegarde des paramètres

        for (int bodiIndex = 0; bodiIndex < bodies.Length; bodiIndex++)
        {
            //assignation vitesses et positions des corps
            positions[bodiIndex] = bodies[bodiIndex].GetDoubleVector3Position();
            velocities[bodiIndex] = bodies[bodiIndex].GetDoubleVector3Velocity();
        }

        playerPosition = playerRB.position;
        playerVelocity = playerRB.linearVelocity;

        bool isKinematic = playerRB.isKinematic;

        if (!isKinematic) { playerRB.isKinematic = true; }
        

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

        //on replace le joueur au centre
        playerRB.position = Vector3.zero;

        //on retabli les paramètres
        
        playerRB.isKinematic = isKinematic;
        playerRB.linearVelocity = playerVelocity; 

        Debug.Log("fin du décalage");
    }

}