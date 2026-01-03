using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunShadowCaster : MonoBehaviour
{
    Transform track;
    public Camera cam;

    void Start()
    {
        track = cam?.transform;
    }

    void LateUpdate()
    {
        if (track)
        {
            transform.LookAt(track.position);
        }
    }
}