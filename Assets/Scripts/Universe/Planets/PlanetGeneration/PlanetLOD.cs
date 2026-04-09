using System;
using UnityEngine;

[RequireComponent(typeof(Planet))]
public class PlanetLOD : MonoBehaviour
{
    public bool useLOD;
    Camera cam;
    Planet planet;

    public int[] currentQuality;
    MeshRenderer[] meshRenderers;
    MeshProperties[] meshProperties;
    MeshFilter[] meshFilters;
    MeshCollider[] meshColliders;

    public void Init(Camera camera)
    {
        cam = camera;
        planet = GetComponent<Planet>();


        meshRenderers = new MeshRenderer[6];
        currentQuality = new int[6];
        meshProperties = new MeshProperties[6];
        meshFilters = new MeshFilter[6];
        meshColliders = new MeshCollider[6];

        //on genere la planete a sa qualté maximale
        planet.InitNoise();
        planet.Generate();

        for (int i = 0; i < 6; i++)
        {
            currentQuality[i] = planet.shape.lowQuality; // LOW
            meshRenderers[i] = planet.MeshChilds[i].GetComponent<MeshRenderer>();
            meshProperties[i] = planet.MeshChilds[i].GetComponent<MeshProperties>();
            meshFilters[i] = planet.MeshChilds[i].GetComponent<MeshFilter>();
            meshColliders[i] = planet.MeshChilds[i].GetComponent<MeshCollider>();
        }

        if (!useLOD) {return;}

        //si c est pas LOD on change pas la qualité
        for (int i = 0; i < 6; i++)
        {
            SetQuality(i, planet.shape.lowQuality);
        }
        

        
    }

    void Update()
    {
        if (!useLOD) return;
        Vector3 pos = cam.transform.position;

        for (int i = 0; i < planet.MeshChilds.Length; i++)
        {


            float margin = 20f;

            float enterHigh  = planet.shape.maxQualityDistance - margin;
            float exitHigh   = planet.shape.maxQualityDistance + margin;

            float enterMedium = planet.shape.mediumQualityDistance - margin;
            float exitMedium  = planet.shape.mediumQualityDistance + margin;
            float sqrDst = (pos - meshRenderers[i].bounds.center).sqrMagnitude;

            if (currentQuality[i] == planet.shape.lowQuality)
            {
                if (sqrDst < enterMedium * enterMedium)
                {
                    SetQuality(i, planet.shape.mediumQuality); // LOW -> MEDIUM
                }
            }
            else if (currentQuality[i] == planet.shape.mediumQuality)
            {
                if (sqrDst < enterHigh * enterHigh)
                {
                    SetQuality(i, planet.shape.maxQuality); // MEDIUM -> HIGH
                }
                else if (sqrDst > exitMedium * exitMedium)
                {
                    SetQuality(i, planet.shape.lowQuality); // MEDIUM -> LOW
                }
            }
            else if (currentQuality[i] == planet.shape.maxQuality)
            {
                if (sqrDst > exitHigh * exitHigh)
                {
                    SetQuality(i, planet.shape.mediumQuality); // HIGH -> MEDIUM
                }
            }
        }
    }

    void SetQuality(int i, int quality)
    {  
        meshProperties[i].quality = quality;
        meshFilters[i].mesh.Clear();        
        (var vertices, var uvArray) = planet.CreateVertices(meshProperties[i].quality, planet.shape.radius, i);
        meshFilters[i].mesh.vertices = vertices;
        meshFilters[i].mesh.uv = uvArray;
        meshFilters[i].mesh.triangles = planet.CreateTriangles(meshProperties[i].quality);
        meshFilters[i].mesh.RecalculateNormals();
        if (planet.shape.planetParameter == PlanetParameter.Solid)
            meshColliders[i].sharedMesh = meshFilters[i].mesh;
        else
            meshColliders[i].sharedMesh = null;
        currentQuality[i] = quality;
    }
}
