using UnityEngine;

[CreateAssetMenu(fileName = "Shape", menuName = "PlanetGeneration/Shape")]
public class Shape : ScriptableObject
{
    [Header("Planet Parameters")]
    public PlanetParameter planetParameter;
    public BodyType bodyType;
    public float radius;

    [Header("LOD Quality")]
    public int maxQuality;
    public int mediumQuality;
    public int lowQuality;

    [Header("LOD Distance")]
    public int maxQualityDistance;
    public int mediumQualityDistance;
    public int lowQualityDistance;

    [Header("Noise")]
    public int seed;
    [Range(-100, 100)] public float noiseScale1;
    [Range(-100, 100)] public float noiseScale2;    

    [Header("Ocean")]
    [Range(0f, 0.5f)] public float oceanScale;
}

public enum PlanetParameter {Solid, Ocean};