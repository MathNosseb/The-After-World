using UnityEngine;

[CreateAssetMenu(fileName = "CratersParameters", menuName = "PlanetGeneration/CratersParameters")]
public class CratersParameters : ScriptableObject
{
    public Texture2D cratertexture;

    public float GetDepthCrater(float x, float y)
    {
        return -cratertexture.GetPixel((int)x % (cratertexture.width -1), (int)y % (cratertexture.height - 1)).grayscale;
    }
}
