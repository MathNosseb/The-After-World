using UnityEngine;

[CreateAssetMenu(fileName = "Shape", menuName = "PlanetGeneration/Shape")]
public class Shape : ScriptableObject
{
    [Header("Planet Parameters")]
    public PlanetParameter planetParameter;
    public float radius;

    [Header("Quality and LOD")]
    [Range(1, 200)] public int quality;
    public int[] facesQuality;

    [Header("Noise")]
    public int seed;
    [Range(-100, 100)] public float noiseScale1;
    [Range(-100, 100)] public float noiseScale2;    

    [Header("Ocean")]
    [Range(0f, 0.5f)] public float oceanScale;
}

public enum PlanetParameter {Solid, Ocean};
