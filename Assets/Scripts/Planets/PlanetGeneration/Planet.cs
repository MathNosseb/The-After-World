using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class Planet : MonoBehaviour
{


    [Range(2, 256)]
    public int resolution = 10;
    public bool usePerlin;
    [Range(0.1f, 1f)]
    public float scale = 1f;
    [Range(0.1f, 10f)]
    public float amplitude = 10f;


    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    MeshFilter meshFilterExport;

    float level = 0;


    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    DestroyAllChild(gameObject); // seulement en édition
                    Initialize();
                    GenerateMesh();
                }
            };
        }
#endif
    }



    [ContextMenu("Clear Mesh")]
    public void SubdiviseMeshEditor()
    {
        level = 0;
        DestroyAllChild(gameObject);
        Initialize();
        GenerateMesh();
        SubdiviseMesh(gameObject, level);
    }

    public void DestroyAllChild(GameObject parent)
    {
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.transform.GetChild(i).gameObject;
            DestroyImmediate(child);
        }
    }


    void Initialize()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();

                
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, directions[i], scale, amplitude, usePerlin);
            MeshCollider meshCollider = meshFilters[i].AddComponent<MeshCollider>();
            meshCollider.sharedMesh = terrainFaces[i].mesh;
        }
    }

    void GenerateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
        
    }
    [ContextMenu("Export Mesh")]
    void ExportMesh()
    {
        string folder = Path.Combine(Application.dataPath, "Exports");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string fileName = "Planet.obj";
        string path = Path.Combine(folder, fileName);
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("# Unity Planet Export");
        int vertexOffset = 0;

        for (int k = 0; k < transform.childCount; k++)
        {
            GameObject child = transform.GetChild(k).gameObject;
            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            Mesh mesh = mf.sharedMesh;
            sb.AppendLine("o " + child.name);

            // Vertices en coordonnées monde
            foreach (Vector3 v in mesh.vertices)
            {
                Vector3 wv = child.transform.TransformPoint(v);
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "v {0} {1} {2}\n", wv.x, wv.y, wv.z);
            }

            // UVs (si présents)
            if (mesh.uv != null && mesh.uv.Length > 0)
            {
                foreach (Vector2 uv in mesh.uv)
                    sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "vt {0} {1}\n", uv.x, 1 - uv.y);
            }

            // Normales (monde)
            if (mesh.normals != null && mesh.normals.Length > 0)
            {
                foreach (Vector3 n in mesh.normals)
                {
                    Vector3 wn = child.transform.TransformDirection(n);
                    sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "vn {0} {1} {2}\n", wn.x, wn.y, wn.z);
                }
            }

            // Faces — inclure indices UV + normales si dispo
            int[] tris = mesh.triangles;
            for (int i = 0; i < tris.Length; i += 3)
            {
                int a = tris[i + 0] + 1 + vertexOffset;
                int b = tris[i + 1] + 1 + vertexOffset;
                int c = tris[i + 2] + 1 + vertexOffset;
                sb.AppendLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");
            }

            vertexOffset += mesh.vertexCount;
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log("✅ Mesh exporté avec succès dans : " + path);
    }

    void SubdiviseMesh(GameObject parent, float level) 
    {
        if (level == 0)
        {
            //si on est sur earth et pas sur les premiers mesh
            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                SubdiviseMesh(parent.transform.GetChild(i).gameObject, level + 1);
            }

        }
        else
        {
            //premiere etape everifier si le parent actuelle a + de 255 faces
            MeshFilter parentMesh = parent.GetComponent<MeshFilter>();
            //suppression de tout les enfants
            DestroyAllChild(parent);
            if (parentMesh.sharedMesh.vertexCount > 255)
            {

                //creation des enfants + subdivision
                MeshSplitter splitter = parent.AddComponent<MeshSplitter>();
                splitter.SplitMesh();

                //passage a la generation suivante
                for (int i = parent.transform.childCount - 1; i >= 0; i--)
                {
                    SubdiviseMesh(parent.transform.GetChild(i).gameObject, level + 1);
                }

            }
        }
    }
}