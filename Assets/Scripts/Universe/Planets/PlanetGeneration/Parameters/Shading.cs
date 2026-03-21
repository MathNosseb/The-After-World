using UnityEngine;

[CreateAssetMenu(fileName = "Shading", menuName = "PlanetGeneration/Shading")]
public class Shading : ScriptableObject
{
    [Header("Material")]
    public Material material;

    [Header("Colors")]
    public Color minColor;
    public Color maxColor;

    [Header("Heights")]
    public float maxHeight;
    public float minHeight;

    [Header("Params")]
    public float metalic;
    public float smoothness;

}
