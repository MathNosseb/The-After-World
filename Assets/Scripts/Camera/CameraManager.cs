using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [HideInInspector]
    public static Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }
}
