using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

[RequireComponent(typeof(PlanetLOD))]
public class Planet : MonoBehaviour
{

    public Shape shape;
    [HideInInspector] public bool shapeFoldout;

    public Shading shading;
    [HideInInspector] public bool shadingFoldout;


    [HideInInspector] public GameObject[] MeshChilds;

    FastNoiseLite continentNoise;
    FastNoiseLite warpNoise;
    FastNoiseLite mountainNoise;
    FastNoiseLite mountainMaskNoise;

    void InitNoise()
    {

        continentNoise = new FastNoiseLite();
        continentNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        continentNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        continentNoise.SetFrequency(0.35f);
        continentNoise.SetFractalOctaves(3);
        continentNoise.SetFractalGain(0.5f);

        warpNoise = new FastNoiseLite();
        warpNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        warpNoise.SetFrequency(0.6f);
        warpNoise.SetFractalOctaves(2);

        mountainNoise = new FastNoiseLite();
        mountainNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        mountainNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        mountainNoise.SetFrequency(1.2f);
        mountainNoise.SetFractalOctaves(5);
        mountainNoise.SetFractalLacunarity(2.2f);
        mountainNoise.SetFractalGain(0.45f);

        mountainMaskNoise = new FastNoiseLite();
        mountainMaskNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        mountainMaskNoise.SetFrequency(0.25f);
        mountainMaskNoise.SetFractalOctaves(2);

        continentNoise.SetSeed(shape.seed);
        warpNoise.SetSeed(shape.seed + 1);
        mountainNoise.SetSeed(shape.seed + 2);
        mountainMaskNoise.SetSeed(shape.seed + 3);
    }

    float GetPlanetHeight(Vector3 p)
    {
        float continent = continentNoise.GetNoise(p.x, p.y, p.z);

        float seaLevel = -shape.oceanScale;

        if (continent < seaLevel)
            return continent * 0.3f * shape.noiseScale1;

        float landMask = Mathf.InverseLerp(seaLevel, seaLevel + 0.3f, continent);
        landMask = Mathf.SmoothStep(0f, 1f, landMask);

        float warpStrength = 0.4f;

        float wx = warpNoise.GetNoise(p.x + 100f, p.y, p.z) * warpStrength;
        float wy = warpNoise.GetNoise(p.x, p.y + 100f, p.z) * warpStrength;
        float wz = warpNoise.GetNoise(p.x, p.y, p.z + 100f) * warpStrength;

        float mountain = mountainNoise.GetNoise(p.x + wx, p.y + wy, p.z + wz);
        mountain = Mathf.Max(0f, mountain);

        float mountainMask = mountainMaskNoise.GetNoise(p.x + 50f, p.y + 50f, p.z + 50f);
        mountainMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.1f, 0.6f, mountainMask));

        float plains    = continent * shape.noiseScale1;
        float mountains = mountain * mountainMask * landMask * shape.noiseScale2 * 3f;

        return plains + mountains;
    }

    public Vector3[] CreateVertices(int presetQuality, float radius, int face)
    {
        NativeArray<Vector3> directions = new NativeArray<Vector3>(6, Allocator.TempJob);
        directions[0] = Vector3.up;
        directions[1] = Vector3.down;
        directions[2] = Vector3.left;
        directions[3] = Vector3.right;
        directions[4] = Vector3.forward;
        directions[5] = Vector3.back;
        
        var job = new GenerateVerticiesJob
        {
            vertices = new NativeArray<Vector3>((presetQuality + 1) * (presetQuality + 1), Allocator.TempJob),
            directions = directions,
            presetQuality = presetQuality,
            face = face,
            radius = radius,
            continentNoise = continentNoise,
            warpNoise = warpNoise,
            mountainNoise = mountainNoise,
            mountainMaskNoise = mountainMaskNoise,
            oceanScale = shape.oceanScale,
            noiseScale1 = shape.noiseScale1,
            noiseScale2 = shape.noiseScale2
        };
        
        var handlejob = job.Schedule((presetQuality + 1) * (presetQuality + 1), 64);
        handlejob.Complete();
        

        Vector3[] verticies = job.vertices.ToArray();
        job.vertices.Dispose();
        directions.Dispose();

        return verticies;

        /*
        
        //if (noise == null) {InitNoise();}
        Debug.Log("[GENERATION] Generation Verticies");
        int verticesPerFace = (presetQuality + 1) * (presetQuality + 1);
        Vector3[] vertices = new Vector3[verticesPerFace];

        Vector3[] directions =
        {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };
        Vector3 localUp = directions[face];
        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);
        int v = 0;
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
        return vertices;
        */
    }

    public int[] CreateTriangles(int presetQuality)
    {
        Debug.Log("[GENERATION] Generation Triangles");
        int faceOffset = 0;
        int[]  triangles = new int[presetQuality * presetQuality * 6];

        int ti = 0;
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

        return triangles;
    }

    public void Generate()
    {
        InitNoise();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        MeshChilds = new GameObject[6];

        for (int f = 0; f < 6; f++)
        {

            GameObject child = new GameObject("mesh " + f);
            MeshChilds[f] = child;
            //Ajout des composants à l'enfant
            child.AddComponent<MeshFilter>();
            child.AddComponent<MeshRenderer>();
            child.AddComponent<MeshCollider>();
            child.AddComponent<MeshProperties>();

            MeshProperties meshProperties;
            meshProperties = child.GetComponent<MeshProperties>();

            meshProperties.quality = shape.maxQuality;

            //setup en temps qu enfant
            child.transform.parent = transform;

            //creation du mesh
            Mesh mesh = new Mesh();
            MeshCollider meshCollider = new MeshCollider();
            child.GetComponent<MeshFilter>().mesh = mesh;
            mesh.name = "Procedural Grid " + f;
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = CreateVertices(meshProperties.quality, shape.radius, f);
            mesh.triangles = CreateTriangles(meshProperties.quality);

            mesh.RecalculateNormals();
            meshCollider = child.GetComponent<MeshCollider>();
            if (shape.planetParameter == PlanetParameter.Solid)
                meshCollider.sharedMesh = mesh;
            else
                meshCollider.sharedMesh = null;

            child.transform.position = transform.position;

            //apply shading
            shading.material.SetColor("_LOWColor", shading.minColor);
            shading.material.SetColor("_HIGHColor", shading.maxColor);
            shading.material.SetFloat("_maxHeight", shading.maxHeight);
            shading.material.SetFloat("_minHeight", shading.minHeight);
            shading.material.SetFloat("_Metalic", shading.metalic);
            shading.material.SetFloat("_Smooth", shading.smoothness);
            child.GetComponent<MeshRenderer>().material = shading.material;


        }
    }

    
}

