using UnityEngine;

[CreateAssetMenu(fileName = "AsteroidData", menuName = "AsteroidData")]
public class AsteroidData : ScriptableObject
{
    [System.Serializable]
    public struct AsteroidContainer
    {
        public Mesh mesh;
        public Color color;
        public Material material;

        [Range(1,1000)]
        public int count;
        public float radius;
        public float rotationSpeed;

        [HideInInspector] public ComputeBuffer bufferArgs;
        [HideInInspector] public ComputeBuffer bufferPositions;
        [HideInInspector] public ComputeBuffer outputPositions;
        
        

    }
    public AsteroidContainer[] asteroidContainer;
    public ComputeShader computeShader;
    [HideInInspector] public int kernel;

}
