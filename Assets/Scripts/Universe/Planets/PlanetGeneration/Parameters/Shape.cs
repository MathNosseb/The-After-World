using UnityEngine;

[CreateAssetMenu(fileName = "Shape", menuName = "PlanetGeneration/Shape")]
public class Shape : ScriptableObject
{
    public float radius;
    [Range(1, 100)] public int quality;

    [Header("Noise")]
    public int seed;
    [Range(-100, 100)] public float noiseScale1;
    [Range(-100, 100)] public float noiseScale2;    
}
