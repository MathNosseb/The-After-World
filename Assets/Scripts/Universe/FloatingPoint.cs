using System.Collections.Generic;
using UnityEngine;

public class FloatingPoint : MonoBehaviour
{
    public dataHolder data;
    public List<Transform> Transforms;
    public GameObject sun;



    public void Awake()
    {
    }

    void FixedUpdate()
    {
        
        
        /*
        if (playerRb.position.magnitude > data.distanceBeforeFloatingPointRepare)
        {
            foreach (var v in Transforms)
            {
                v.transform.position += offset;
            }
            
        }
        */

        //detecttion du floating subject
        GameObject subject = data.GetSubjectGameObject();
        Rigidbody subjectRb = data.GetSubjectRigidbody();

        Vector3 offset = -subject.transform.position;
               
        if (subjectRb.position.magnitude > data.distanceBeforeFloatingPointRepare)
        {
            Debug.Log("position " + subjectRb.position.magnitude);

            Debug.Log("offset " +  offset);
            Vector3 velocity = subjectRb.linearVelocity;
            sun.GetComponent<CelestialBody>().ChangePosition(sun.transform.position + offset);
            bool state = subjectRb.isKinematic;    
            if (!subjectRb.isKinematic)
                subjectRb.isKinematic = true;
            subject.transform.position = Vector3.zero;
            subjectRb.isKinematic = state;
            subjectRb.linearVelocity = velocity;    
            
        }

        


    }



}
