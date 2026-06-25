using UnityEngine;

public class LookAt : MonoBehaviour
{
    public GameObject cible;

    void Update()
    {
        transform.LookAt(cible.transform); 
    }
}
