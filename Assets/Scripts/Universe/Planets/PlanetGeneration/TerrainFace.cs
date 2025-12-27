using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Text;

public class TerrainFace
{
    [HideInInspector]
    public Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float scale;
    float amplitude;
    bool usePerlin;


    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp,float scale,float amplitude, bool usePerlin)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.scale = scale;
        this.usePerlin = usePerlin;
        this.amplitude = amplitude;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
        
    }

    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        Color[] colors = new Color[resolution * resolution];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;

                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                if (usePerlin)
                {
                    float elevation = Mathf.PerlinNoise(pointOnUnitSphere.x + 100f * scale, pointOnUnitSphere.y + 100f * scale) + Mathf.PerlinNoise(pointOnUnitSphere.x + 100f * scale/10, pointOnUnitSphere.y + 100f * scale/10);
                    pointOnUnitSphere *= (15 + elevation * amplitude);
                }
                else
                {
                    pointOnUnitSphere *= 10; // rayon de base
                }

                vertices[i] = pointOnUnitSphere;

                // --- COLORATION ---
                float height = pointOnUnitSphere.magnitude; // distance au centre
                float minHeight = 10f;                       // rayon de base
                float maxHeight = 15 + amplitude * 2f;       // max avec Perlin
                float normHeight = Mathf.InverseLerp(minHeight, maxHeight, height);

                if (normHeight < 0.6f)
                    colors[i] = new Color(67f / 255f, 117f / 255f, 79f / 255f);   
                else if (normHeight < 0.7f)
                    colors[i] = new Color(36f / 255f, 88f / 255f, 48f / 255f);    
                else
                    colors[i] = new Color(25f / 255f, 75f / 255f, 37f / 255f);


                // Triangles
                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();

    }

}