using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Grass", menuName = "PlanetGeneration/Grass")]
public class Grass : ScriptableObject
{
    public Material mat;
    [HideInInspector] public GameObject[] surface;
    public Color color;

    [HideInInspector] public Mesh grassMesh;

    [HideInInspector] public GameObject sun;
    [HideInInspector] public CelestialBody sunCelestial;

    [HideInInspector] public ComputeBuffer[] positionsBuffer;
    [HideInInspector] public ComputeBuffer[] argsBuffer;
    [HideInInspector] public ComputeBuffer[] rotationBuffer;
    [HideInInspector] public ComputeBuffer[] noiseBuffer;

    [HideInInspector] public CommandBuffer cmd;

    [Range(1, 100)] public int density;
    [Range(1,2)] public float spread;

    public float minDistance;
    [Range(1,20)] public float maxDistance;

    public CameraEvent cameraEvent;

    [HideInInspector] public bool[] faceInit;

    
}
