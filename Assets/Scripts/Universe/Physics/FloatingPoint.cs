using System.Collections.Generic;
using UnityEngine;

public class FloatingPoint : MonoBehaviour
{
    public List<Transform> Planets;
    public List<Transform> OtherObjects;
    public GameObject sun;

    public Rigidbody body;

    public int distance;

    public Vector3 initialVeclocity;


    bool floatingShiftThisFrame = false;
    bool pendingReenable = false;

    Vector3 velocity;
    bool state;

    void FixedUpdate()
    {
        GameObject subject = body.gameObject;
        Rigidbody subjectRb = body;

        Vector3 offset = -subjectRb.position;

        // 1️⃣ Déclenchement du floating origin
        if (!floatingShiftThisFrame && !pendingReenable &&
            subjectRb.position.magnitude > distance)
        {
            floatingShiftThisFrame = true;
            pendingReenable = true;

            // Sauvegarder l'état du Rigidbody
            velocity = subjectRb.linearVelocity;
            state = subjectRb.isKinematic;

            // Stopper la physique pendant le shift
            subjectRb.isKinematic = true;

            // Déplacer toutes les planètes
            foreach (var v in Planets)
                v.GetComponent<CelestialBody>()
                 .ChangePosition(v.transform.position + offset);

            // Déplacer tous les autres objets
            foreach (var v in OtherObjects)
                v.GetComponent<Rigidbody>().position =
                    v.transform.position + offset;

            // Repositionner le joueur
            subjectRb.position = Vector3.zero;

            return; // Fin de la frame neutre
        }

        // 2️⃣ Frame suivante : réactiver la physique
        if (pendingReenable)
        {
            subjectRb.isKinematic = state;
            subjectRb.linearVelocity = velocity;

            pendingReenable = false;
            floatingShiftThisFrame = false;
        }
    }

    /*   
    if (subjectRb.position.magnitude > distance)
    {
        Debug.Log("position " + subjectRb.position.magnitude);

        Debug.Log("offset " +  offset);
        Vector3 velocity = subjectRb.linearVelocity;
        sun.GetComponent<CelestialBody>().ChangePosition(sun.transform.position + offset);
        subject.transform.position = Vector3.zero;




        bool state = subjectRb.isKinematic;    
        if (!subjectRb.isKinematic)
            subjectRb.isKinematic = true;
        subject.transform.position = Vector3.zero;
        subjectRb.isKinematic = state;
        subjectRb.linearVelocity = velocity;    
        */




}
