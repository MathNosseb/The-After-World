using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Planet : MonoBehaviour
{
    
    public int seed;
    public float size;
    [Range(1,100)]
    public int quality;
    
    public bool automaticGenerate;

    [Header("noise")]
    
    [Range(0.1f,100)] public float noiseScale1;
    [Range(0.1f,100)] public float noiseScale2;
    

    private FastNoiseLite _noise;
    MeshCollider meshCollider;
    private Mesh mesh;


    int[] triangles;

    void OnValidate()
    {
        if (automaticGenerate)
            Generate();
    }

    void InitNoise()
    {
        _noise = new FastNoiseLite();
        _noise.SetSeed(seed);
    }

    float GetPlanetHeight(Vector3 p)
    {
        // ─── ÉTAPE 1 : Forme des continents ───────────────────────────────
        // Basse fréquence, peu d'octaves → grandes masses terrestres
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _noise.SetFrequency(0.35f);
        _noise.SetFractalOctaves(3);
        _noise.SetFractalGain(0.5f);
        float continent = _noise.GetNoise(p.x, p.y, p.z); // [-1, 1]

        // ─── ÉTAPE 2 : Océans plats ────────────────────────────────────────
        // Tout ce qui est sous le seuil = fond plat (pas de bruit sous l'eau)
        float seaLevel = -0.05f;
        if (continent < seaLevel)
            return continent * 0.3f * noiseScale1; // fond marin légèrement vallonné

        // ─── ÉTAPE 3 : Plaines ────────────────────────────────────────────
        // Continent émergé mais "écrasé" → zones plates
        // SmoothStep aplatit les valeurs proches du niveau de la mer
        float landMask = Mathf.InverseLerp(seaLevel, seaLevel + 0.3f, continent);
        landMask = Mathf.SmoothStep(0f, 1f, landMask); // courbe en S → transition douce

        // ─── ÉTAPE 4 : Montagnes ──────────────────────────────────────────
        // Ridged noise haute fréquence, mais MULTIPLIÉ par le masque
        // → montagnes absentes sur les plaines, présentes sur les hauts plateaux
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        _noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        _noise.SetFrequency(1.2f);
        _noise.SetFractalOctaves(5);
        _noise.SetFractalLacunarity(2.2f);
        _noise.SetFractalGain(0.45f);

        // Domain warp : déforme les coordonnées avant d'échantillonner
        // → les chaînes de montagnes s'incurvent naturellement
        float warpStrength = 0.4f;
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _noise.SetFrequency(0.6f);
        _noise.SetFractalOctaves(2);
        float wx = _noise.GetNoise(p.x + 100f, p.y, p.z) * warpStrength;
        float wy = _noise.GetNoise(p.x, p.y + 100f, p.z) * warpStrength;
        float wz = _noise.GetNoise(p.x, p.y, p.z + 100f) * warpStrength;

        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        _noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        _noise.SetFrequency(1.2f);
        _noise.SetFractalOctaves(5);
        float mountain = _noise.GetNoise(p.x + wx, p.y + wy, p.z + wz);
        mountain = Mathf.Max(0f, mountain); // supprime les valeurs négatives

        // ─── ÉTAPE 5 : Masque de montagne séparé ──────────────────────────
        // Les montagnes n'apparaissent que dans certaines zones (pas partout)
        _noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _noise.SetFrequency(0.25f);
        _noise.SetFractalOctaves(2);
        float mountainMask = _noise.GetNoise(p.x + 50f, p.y + 50f, p.z + 50f);
        mountainMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.1f, 0.6f, mountainMask));

        // ─── ASSEMBLAGE FINAL ─────────────────────────────────────────────
        float plains    = continent * noiseScale1;                         // terrain de base plat
        float mountains = mountain  * mountainMask * landMask * noiseScale2 * 3f; // relief masqué

        return plains + mountains;
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        GetComponent<MeshFilter>().mesh = null;
    }


    Vector3[] CreateVertices(int presetQuality, float radius)
    {
        if (_noise == null) {InitNoise();}
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
                    float height = Mathf.Max(0f, noise) * noiseScale1;
                    vertices[v++] = point * (radius + noise);
                }
            }
        }

        return vertices;
    }

    int[] CreateTriangles(int presetQuality)
    {
        triangles = new int[presetQuality * presetQuality * 6 * 6];

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
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";
        
        mesh.vertices = CreateVertices(quality,size);
        
        mesh.triangles = CreateTriangles(quality);
        mesh.RecalculateNormals();
        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        
    }

    
}

