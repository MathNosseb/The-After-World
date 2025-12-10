using System.Collections;
using UnityEngine;

public class BackgroundCam : MonoBehaviour
{
    public Transform[] positions; // les objets à suivre
    public float waitTime = 1f;   // temps entre changement d'objet

    void Start()
    {
        StartCoroutine(FollowObjectsLoop());
    }

    IEnumerator FollowObjectsLoop()
    {
        int index = 0;

        while (true)
        {
            Transform target = positions[index]; // objet actuel à suivre

            float timer = 0f;
            while (timer < waitTime)
            {
                // Suivre la position et rotation en temps réel
                transform.position = target.position;
                transform.rotation = target.rotation;

                timer += Time.deltaTime;
                yield return null;
            }

            // Passer à l'objet suivant en boucle
            index = (index + 1) % positions.Length;
        }
    }
}
