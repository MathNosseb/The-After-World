using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Planet : MonoBehaviour
{

    public Shape shape;
    [HideInInspector] public bool shapeFoldout;

    public Shading shading;
    [HideInInspector] public bool shadingFoldout;

    FastNoiseLite noise;
    MeshCollider meshCollider;
    Mesh mesh;


    void InitNoise()
    {
        noise = new FastNoiseLite();
        noise.SetSeed(shape.seed);
    }

    float GetPlanetHeight(Vector3 p)
    {
        // ─── ÉTAPE 1 : Forme des continents ───────────────────────────────
        // Basse fréquence, peu d'octaves → grandes masses terrestres
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFrequency(0.35f);
        noise.SetFractalOctaves(3);
        noise.SetFractalGain(0.5f);
        float continent = noise.GetNoise(p.x, p.y, p.z); // [-1, 1]

        // ─── ÉTAPE 2 : Océans plats ────────────────────────────────────────
        // Tout ce qui est sous le seuil = fond plat (pas de bruit sous l'eau)
        float seaLevel = -shape.oceanScale;
        if (continent < seaLevel)
            return continent * 0.3f * shape.noiseScale1; // fond marin légèrement vallonné

        // ─── ÉTAPE 3 : Plaines ────────────────────────────────────────────
        // Continent émergé mais "écrasé" → zones plates
        // SmoothStep aplatit les valeurs proches du niveau de la mer
        float landMask = Mathf.InverseLerp(seaLevel, seaLevel + 0.3f, continent);
        landMask = Mathf.SmoothStep(0f, 1f, landMask); // courbe en S → transition douce

        // ─── ÉTAPE 4 : Montagnes ──────────────────────────────────────────
        // Ridged noise haute fréquence, mais MULTIPLIÉ par le masque
        // → montagnes absentes sur les plaines, présentes sur les hauts plateaux
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        noise.SetFrequency(1.2f);
        noise.SetFractalOctaves(5);
        noise.SetFractalLacunarity(2.2f);
        noise.SetFractalGain(0.45f);

        // Domain warp : déforme les coordonnées avant d'échantillonner
        // → les chaînes de montagnes s'incurvent naturellement
        float warpStrength = 0.4f;
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(0.6f);
        noise.SetFractalOctaves(2);
        float wx = noise.GetNoise(p.x + 100f, p.y, p.z) * warpStrength;
        float wy = noise.GetNoise(p.x, p.y + 100f, p.z) * warpStrength;
        float wz = noise.GetNoise(p.x, p.y, p.z + 100f) * warpStrength;

        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        noise.SetFrequency(1.2f);
        noise.SetFractalOctaves(5);
        float mountain = noise.GetNoise(p.x + wx, p.y + wy, p.z + wz);
        mountain = Mathf.Max(0f, mountain); // supprime les valeurs négatives

        // ─── ÉTAPE 5 : Masque de montagne séparé ──────────────────────────
        // Les montagnes n'apparaissent que dans certaines zones (pas partout)
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFrequency(0.25f);
        noise.SetFractalOctaves(2);
        float mountainMask = noise.GetNoise(p.x + 50f, p.y + 50f, p.z + 50f);
        mountainMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.1f, 0.6f, mountainMask));

        // ─── ASSEMBLAGE FINAL ─────────────────────────────────────────────
        float plains    = continent * shape.noiseScale1;                         // terrain de base plat
        float mountains = mountain  * mountainMask * landMask * shape.noiseScale2 * 3f; // relief masqué

        return plains + mountains;
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        GetComponent<MeshFilter>().mesh = null;
    }


    Vector3[] CreateVertices(int presetQuality, float radius)
    {
        if (noise == null) {InitNoise();}
        int verticesPerFace = (presetQuality + 1) * (presetQuality + 1);
        Vector3[] vertices = new Vector3[verticesPerFace * 6];

        Vector3[] directions =
        {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

        int v = 0;

        for (int f = 0; f < 6; f++)
        {
            Vector3 localUp = directions[f];
            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 axisB = Vector3.Cross(localUp, axisA);

            for (int y = 0; y <= presetQuality; y++)
            {
                for (int x = 0; x <= presetQuality; x++)
                {
                    Vector2 percent = new Vector2(x, y) / presetQuality;

                    // position sur le cube
                    Vector3 point = localUp +
                                    (percent.x - 0.5f) * 2f * axisA +
                                    (percent.y - 0.5f) * 2f * axisB;

                    // normalisation pour passer sur la sphère
                    point = point.normalized;

                    // GetNoise prend directement X Y Z → 0 couture, 0 artefact
                    float noise = GetPlanetHeight(point);

                    // noise est dans [-1, 1] → on aplatit les océans
                    vertices[v++] = point * (radius + noise);
                }
            }
        }
        return vertices;
    }

    int[] CreateTriangles(int presetQuality)
    {
        int[]  triangles = new int[presetQuality * presetQuality * 6 * 6];

        int ti = 0;
        int faceOffset = 0;

        for (int f = 0; f < 6; f++)
        {
            for (int y = 0; y < presetQuality; y++)
            {
                for (int x = 0; x < presetQuality; x++)
                {
                    int vi = faceOffset + x + y * (presetQuality + 1);

                    triangles[ti++] = vi;
                    triangles[ti++] = vi + 1;
                    triangles[ti++] = vi + presetQuality + 1;

                    triangles[ti++] = vi + 1;
                    triangles[ti++] = vi + presetQuality + 2;
                    triangles[ti++] = vi + presetQuality + 1;
                }
            }

            faceOffset += (presetQuality + 1) * (presetQuality + 1);
        }

        return triangles;
    }

    [ContextMenu("Regenerate Dots")]
    public void Generate()
    {
        InitNoise();
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshRenderer>().material = shading.material;
        mesh.name = "Procedural Grid";

        Debug.Log(shape.quality + shape.radius);
        Vector3[] rawVerts = CreateVertices(shape.quality, shape.radius);
        int[]     rawTris  = CreateTriangles(shape.quality);

        mesh.vertices  = rawVerts;
        mesh.triangles = rawTris;
        mesh.RecalculateNormals();
        meshCollider = GetComponent<MeshCollider>();
        if (shape.planetParameter == PlanetParameter.Solid)
            meshCollider.sharedMesh = mesh;
        else
            meshCollider.sharedMesh = null;
        
    }

    
}

