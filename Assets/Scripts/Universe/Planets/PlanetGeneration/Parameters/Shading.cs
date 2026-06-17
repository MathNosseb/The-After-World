using UnityEngine;

[CreateAssetMenu(fileName = "Shading", menuName = "PlanetGeneration/Shading")]
public class Shading : ScriptableObject
{
    [Header("Material")]
    public Material material;

    [Header("Textures")]
    public Texture2D minTexture;
    public Texture2D maxTexture;

    [Header("Heights")]
    public float maxHeight;
    public float minHeight;

    [Header("Params")]
    public float metalic;
    public float smoothness;
    public Vector2 Tiling = new Vector2(1,1);
    public Vector2 Offset;

}
