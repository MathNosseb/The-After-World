#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class PerlinNoise : MonoBehaviour
{
    static public int width = 256;
    static public int height = 256;

    static public float zoom = 2;
    

    [MenuItem("Tools/Create Perlin Noise texture")]
    static void CreatePerlinNoise()
    {
        Texture2D noiseTexture = GenerateTexture();
        SaveAsPNG(noiseTexture, Application.dataPath + "/Import/2DTextures/PerlinNoise/SavedTexture.png");
    }
    static Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    static Color CalculateColor(int x, int y)
    {
        float xCoord = (float)x / width * zoom;
        float yCoord = (float)y / height * zoom;

        float sample =  Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }

    static void SaveAsPNG(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log("Texture sauvegardée : " + path);
    }
}
#endif