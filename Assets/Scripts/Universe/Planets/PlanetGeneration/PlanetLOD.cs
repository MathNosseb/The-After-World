using UnityEngine;

[RequireComponent(typeof(Planet))]
public class PlanetLOD : MonoBehaviour
{
    public bool useLOD;
    Camera cam;
    Planet planet;

    int[] currentQuality;
    MeshRenderer[] meshRenderers;

    void Start()
    {
        cam = Camera.main;
        planet = GetComponent<Planet>();
        currentQuality = new int[6];
        for (int i = 0; i < currentQuality.Length; i++)
            currentQuality[i] = 10; // LOW
        planet.Generate();
        for (int i = 0; i < 6; i++)
        {
            SetQuality(i, planet.shape.baseQuality);
        }
        meshRenderers = new MeshRenderer[6];

        
    }

    void Update()
    {
        if (!useLOD) return;
        Vector3 pos = cam.transform.position;

        for (int i = 0; i < planet.MeshChilds.Length; i++)
        {
            if (meshRenderers[i] == null)
            {
                meshRenderers[i] = planet.MeshChilds[i].GetComponent<MeshRenderer>();
            }

            float enterHigh = planet.shape.radius-20f;
            float exitHigh  = planet.shape.radius+120f;
            float sqrDst = (pos - meshRenderers[i].bounds.center).sqrMagnitude;

            if (currentQuality[i] == 10 && sqrDst < enterHigh * enterHigh)
            {
                // passe en HIGH
                SetQuality(i, 100);
            }
            else if (currentQuality[i] == 100 && sqrDst > exitHigh * exitHigh)
            {
                // repasse en LOW
                SetQuality(i, 10);
            }
        }
    }

    void SetQuality(int i, int quality)
    {
        Debug.Log($"[UPDATE] Chunk {i} de {gameObject.name} : quality = {quality}");
        MeshProperties meshProperties;
        meshProperties = planet.MeshChilds[i].GetComponent<MeshProperties>();
        meshProperties.quality = quality;
        Mesh mesh = planet.MeshChilds[i].GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        
        mesh.vertices = planet.CreateVertices(meshProperties.quality, planet.shape.radius, i);
        mesh.triangles = planet.CreateTriangles(meshProperties.quality);
        mesh.RecalculateNormals();
        planet.MeshChilds[i].GetComponent<MeshCollider>().sharedMesh = mesh;
        currentQuality[i] = quality;
    }
}
