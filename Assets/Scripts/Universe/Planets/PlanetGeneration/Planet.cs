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

    public bool useGrass = false;
    public Grass grass;
    public GrassMeshData grassMeshData;

    public bool useCrater;
    public CratersParameters cratersParameters;

    public void InitNoise()
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


    public Vector3[] CreateVertices(int presetQuality, float radius, int face)
    {
        NativeArray<Vector3> directions = new NativeArray<Vector3>(6, Allocator.TempJob);
        directions[0] = Vector3.up;
        directions[1] = Vector3.down;
        directions[2] = Vector3.left;
        directions[3] = Vector3.right;
        directions[4] = Vector3.forward;
        directions[5] = Vector3.back;

        NativeArray<float> cratersDepth = new NativeArray<float>((presetQuality + 1) * (presetQuality + 1), Allocator.TempJob);
        if (useCrater)
        {
            for (int cratersPixel = 0; cratersPixel < (presetQuality + 1) * (presetQuality + 1); cratersPixel++)
            {

                cratersDepth[cratersPixel] = cratersParameters.GetDepthCrater(cratersPixel% (presetQuality + 1), cratersPixel/ (presetQuality + 1));
            }
        }
        
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
            noiseScale2 = shape.noiseScale2,
            cratersDepth = cratersDepth,
            useCrater = useCrater ? 1 : 0
        };
        
        var handlejob = job.Schedule((presetQuality + 1) * (presetQuality + 1), 64);
        handlejob.Complete();
        

        Vector3[] verticies = job.vertices.ToArray();
        job.vertices.Dispose();
        directions.Dispose();
        cratersDepth.Dispose();

        return verticies;
    }

    public int[] CreateTriangles(int presetQuality)
    {
        int[] triangles = new int[presetQuality * presetQuality * 6];

        int ti = 0;
        for (int y = 0; y < presetQuality; y++)
        {
            for (int x = 0; x < presetQuality; x++)
            {
                int vi = x + y * (presetQuality + 1);

                triangles[ti++] = vi;
                triangles[ti++] = vi + 1;
                triangles[ti++] = vi + presetQuality + 1;

                triangles[ti++] = vi + 1;
                triangles[ti++] = vi + presetQuality + 2;
                triangles[ti++] = vi + presetQuality + 1;
            }
        }
        return triangles;
    }

    public void Generate()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        MeshChilds = new GameObject[6];
        if (useGrass) 
        {
            grass.surface = new GameObject[6];
            grass.positionsBuffer = new ComputeBuffer[6];
            grass.argsBuffer = new ComputeBuffer[6];
            grass.rotationBuffer = new ComputeBuffer[6];
            grass.noiseBuffer = new ComputeBuffer[6];
            grass.faceInit = new bool[6];
            grass.bladeCounts = new int[6];
            grass.outputBladeData = new ComputeBuffer[6];
            grass.meshRenderers = new MeshRenderer[6];

            grass.kernel = grass.computeShader.FindKernel("CSMain");
            
        }
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

            if (useGrass) 
            {
                grass.surface[f] = child;
                grass.meshRenderers[f] = child.GetComponent<MeshRenderer>();
                GetComponent<GenerateGrassTerrain>().SetUpGrass(f, grass, grassMeshData);
            }
        }
    }

    
}

