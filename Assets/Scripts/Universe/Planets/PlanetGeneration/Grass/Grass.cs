using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Grass", menuName = "PlanetGeneration/Grass")]
public class Grass : ScriptableObject
{   
    public struct BladeData {
        public Vector3 position;
        public Quaternion rotation;
        public float noise;
    };

    public ComputeShader computeShader;
    [HideInInspector] public int kernel;
    public Material[] mat;
    [HideInInspector] public GameObject[] surface;
    public Color color;

    [HideInInspector] public Mesh grassMesh;

    [HideInInspector] public GameObject sun;
    [HideInInspector] public CelestialBody sunCelestial;

    //sortie post Generation CPU
    [HideInInspector] public ComputeBuffer[] positionsBuffer;
    [HideInInspector] public ComputeBuffer[] argsBuffer;
    [HideInInspector] public ComputeBuffer[] rotationBuffer;
    [HideInInspector] public ComputeBuffer[] noiseBuffer;

    //la sortie post culling GPU
    [HideInInspector] public ComputeBuffer[] outputBladeData;

    [HideInInspector] public int[] bladeCounts;

    [HideInInspector] public CommandBuffer cmd;

    [Range(1, 100)] public int density;
    [Range(1,2)] public float spread;

    public float minDistance;
    [Range(1,20)] public float maxDistance;

    [Range(1,100)] public float viewDistance;

    public CameraEvent cameraEvent;

    [HideInInspector] public bool[] faceInit;

    [Range(0,0.3f)]
    public float cropEffect;

    
}
