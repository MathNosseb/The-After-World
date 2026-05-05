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


    //les differents noises -> burst compile compatible
    FastNoiseLite continentNoise;
    FastNoiseLite warpNoise;
    FastNoiseLite mountainNoise;
    FastNoiseLite mountainMaskNoise;

    //utilisation de l herbe
    public bool useGrass = false;
    public Grass grass;
    public GrassMeshData grassMeshData;


    /// <summary>
    /// setup du noise
    /// creation de differents types de noise en fonction de si on est 
    /// dans une montagne, une plaine, un ocean
    /// </summary>
    public void InitNoise()
    {
        //set up du noise des continents
        continentNoise = new FastNoiseLite();
        continentNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        continentNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        continentNoise.SetFrequency(0.35f);
        continentNoise.SetFractalOctaves(3);
        continentNoise.SetFractalGain(0.5f);

        //setup du noise de warp
        warpNoise = new FastNoiseLite();
        warpNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        warpNoise.SetFrequency(0.6f);
        warpNoise.SetFractalOctaves(2);

        //setup du noise des montagnes
        mountainNoise = new FastNoiseLite();
        mountainNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        mountainNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        mountainNoise.SetFrequency(1.2f);
        mountainNoise.SetFractalOctaves(5);
        mountainNoise.SetFractalLacunarity(2.2f);
        mountainNoise.SetFractalGain(0.45f);

        //setup du noise du mask des montagnes
        mountainMaskNoise = new FastNoiseLite();
        mountainMaskNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        mountainMaskNoise.SetFrequency(0.25f);
        mountainMaskNoise.SetFractalOctaves(2);

        //setup des seeds des noises
        continentNoise.SetSeed(shape.seed);
        warpNoise.SetSeed(shape.seed + 1);
        mountainNoise.SetSeed(shape.seed + 2);
        mountainMaskNoise.SetSeed(shape.seed + 3);
    }

    /// <summary>
    /// generation des points des planetes en fonction des paramatres d entrée
    /// </summary>
    /// <param name="presetQuality">la qualité de la génération</param>
    /// <param name="radius">le radius de la planete</param>
    /// <param name="face">la face du carre normalise</param>
    /// <returns>retourne les coos des points et les uv des points</returns>
    public (Vector3[],Vector2[]) CreateVertices(int presetQuality, float radius, int face)
    {
        int nbrVertices = (presetQuality + 1) * (presetQuality + 1);
        //les differentes directions 
        NativeArray<Vector3> directions = new NativeArray<Vector3>(6, Allocator.TempJob);
        directions[0] = Vector3.up;
        directions[1] = Vector3.down;
        directions[2] = Vector3.left;
        directions[3] = Vector3.right;
        directions[4] = Vector3.forward;
        directions[5] = Vector3.back;

        //centre du cratere -> correspond au centre de la face en world space
        Vector3 craterCenter = transform.position + directions[face] * radius;
        var job = new GenerateVerticiesJob
        {
            vertices = new NativeArray<Vector3>(nbrVertices, Allocator.TempJob),//array output vertices
            uvs = new NativeArray<Vector2>(nbrVertices, Allocator.TempJob),//array output uv
            directions = directions,//directions de toutes les faces
            presetQuality = presetQuality,//qualité de la face
            face = face,//face actuelle
            radius = radius,//radius de la planete
            continentNoise = continentNoise,//noise des continents
            warpNoise = warpNoise,//noise warp
            mountainNoise = mountainNoise,//noise des montagnes
            mountainMaskNoise = mountainMaskNoise,//noise montain mask
            oceanScale = shape.oceanScale,//taille de l ocean
            noiseScale1 = shape.noiseScale1,//taille des gros details (montagnes)
            noiseScale2 = shape.noiseScale2,//taille des petits details (piques)
            moon = shape.bodyType == BodyType.Moon ? 1 : 0,//definition montagne
            craterCenter = craterCenter//position du centre de la face de la planete
        };
        
        //execution du job
        var handlejob = job.Schedule(nbrVertices, 64);
        handlejob.Complete();
        
        //convertion des NativeArray en Array
        Vector3[] verticies = job.vertices.ToArray();
        Vector2[] uvs = job.uvs.ToArray();

        //on libere les natives array
        job.uvs.Dispose();
        job.vertices.Dispose();
        directions.Dispose();

        return (verticies,uvs);
    }

    /// <summary>
    /// crée les trianges des faces des corps celeste
    /// </summary>
    /// <param name="presetQuality">qualité de la planete</param>
    /// <returns>liste des triangles du mesh</returns>
    public int[] CreateTriangles(int presetQuality)
    {
        //liste initiale de triangles vide
        //attention a la conso de ram -> mais éléminé par le GC
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

    /// <summary>
    /// Generation du corp céleste
    /// </summary>
    public void Generate()
    {
        //on detruit tous enfants
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);

        }

        //6 faces représenté par 6 gameObjects
        MeshChilds = new GameObject[6];

        //setup de l herbe
        if (useGrass) 
        {
            grass.surface = new GameObject[6];//les 6 faces
            grass.positionsBuffer = new ComputeBuffer[6];//le buffer poistion
            grass.argsBuffer = new ComputeBuffer[6];//le buffer arguments
            grass.rotationBuffer = new ComputeBuffer[6];//le buffer orientation
            grass.noiseBuffer = new ComputeBuffer[6];//le buffer du noise
            grass.faceInit = new bool[6];//si la face est init
            grass.bladeCounts = new int[6];//le nombre de brins d herbe par faces en raw
            grass.outputBladeData = new ComputeBuffer[6];//buffer struct avec les data apres compute shader
            grass.meshRenderers = new MeshRenderer[6];//renderer de l herbe

            //kernel du compute shader de l herbe
            grass.kernel = grass.computeShader.FindKernel("CSMain");
            
        }
        //boucle sur chacune des faces
        for (int f = 0; f < 6; f++)
        {
            //creation du gameObject
            GameObject child = new GameObject("mesh " + f);
            MeshChilds[f] = child;//ajout du gameObject a la liste de GameOject
            
            //Ajout des composants à l'enfant
            child.AddComponent<MeshFilter>();
            child.AddComponent<MeshRenderer>();
            child.AddComponent<MeshCollider>();
            child.AddComponent<MeshProperties>();

            //les properties du mesh (qualité)
            MeshProperties meshProperties = child.GetComponent<MeshProperties>();
            //on applique la qualité maximal
            meshProperties.quality = shape.maxQuality;

            //setup en temps qu enfant
            child.transform.parent = transform;

            Mesh mesh = new Mesh();//creation du mesh
            MeshCollider meshCollider = new MeshCollider();//creation du collider
            child.GetComponent<MeshFilter>().mesh = mesh;//application du mesh
            mesh.name = "Procedural Grid " + f;//nom du mesh
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;//
            (var vertices, var uvArray) = CreateVertices(meshProperties.quality, shape.radius, f);
            mesh.vertices = vertices;
            mesh.uv = uvArray;
            
            mesh.triangles = CreateTriangles(meshProperties.quality);

            mesh.RecalculateNormals();
            meshCollider = child.GetComponent<MeshCollider>();
            if (shape.planetParameter == PlanetParameter.Solid)
                meshCollider.sharedMesh = mesh;
            else
                meshCollider.sharedMesh = null;

            child.transform.position = transform.position;

            if (shape.planetParameter == PlanetParameter.Solid)
            {
                //apply shading

                shading.material.SetTexture("_LOWTexture", shading.minTexture);
                shading.material.SetTexture("_HIGHTexture", shading.maxTexture);
                shading.material.SetVector("_Tiling", shading.Tiling);
                shading.material.SetVector("_Offset", shading.Offset);
                shading.material.SetFloat("_maxHeight", shading.maxHeight);
                shading.material.SetFloat("_minHeight", shading.minHeight);
                shading.material.SetFloat("_Metalic", shading.metalic);
                shading.material.SetFloat("_Smooth", shading.smoothness);
                
            }
            
            child.GetComponent<MeshRenderer>().material = shading.material;
            if (useGrass) 
            {
                grass.surface[f] = child;
                grass.meshRenderers[f] = child.GetComponent<MeshRenderer>();
                grass.continentNoise = continentNoise;
                grass.warpNoise = warpNoise;
                grass.mountainNoise = mountainNoise;
                grass.mountainMaskNoise = mountainMaskNoise;

                grass.oceanScale = shape.oceanScale;
                grass.noiseScale1 = shape.noiseScale1;
                grass.noiseScale2 = shape.noiseScale2;

                grass.planetRadius = shape.radius;
                grass.seed = shape.seed;

                grass.directions = new Vector3[6];
                grass.directions[0] = Vector3.up;
                grass.directions[1] = Vector3.down;
                grass.directions[2] = Vector3.left;
                grass.directions[3] = Vector3.right;
                grass.directions[4] = Vector3.forward;
                grass.directions[5] = Vector3.back;

                GetComponent<GenerateGrassTerrain>().SetUpGrass(f, grass, grassMeshData);
            }
        }
    }

    
}

