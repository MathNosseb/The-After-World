using UnityEngine;

[ExecuteInEditMode]
public class CameraManager : MonoBehaviour
{

    public Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }
}
