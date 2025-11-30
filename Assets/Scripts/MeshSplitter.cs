using UnityEngine;

public class MeshSplitter : MonoBehaviour
{
    public void SplitMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("Pas de MeshFilter ou de Mesh trouvé sur ce GameObject.");
            return;
        }

        Mesh originalMesh = mf.sharedMesh;


        Vector3[] vertices = originalMesh.vertices;
        int[] triangles = originalMesh.triangles;

        // Création des deux objets vides
        GameObject child1 = new GameObject("Part1");
        GameObject child2 = new GameObject("Part2");

        child1.transform.parent = transform;
        child2.transform.parent = transform;

        child1.transform.position = transform.position;
        child2.transform.position = transform.position;

        // Mesh pour chaque partie
        Mesh mesh1 = new Mesh();
        Mesh mesh2 = new Mesh();

        // On va séparer selon l'axe Y (milieu)
        float centerY = 0f;
        foreach (Vector3 v in vertices) centerY += v.y;
        centerY /= vertices.Length;

        // Listes temporaires pour les deux meshes
        var verts1 = new System.Collections.Generic.List<Vector3>();
        var tris1 = new System.Collections.Generic.List<int>();

        var verts2 = new System.Collections.Generic.List<Vector3>();
        var tris2 = new System.Collections.Generic.List<int>();

        // On parcourt les triangles
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            // Calcul de la moyenne Y du triangle
            float triCenterY = (v0.y + v1.y + v2.y) / 3f;

            if (triCenterY > centerY)
            {
                int baseIndex = verts1.Count;
                verts1.Add(v0); verts1.Add(v1); verts1.Add(v2);
                tris1.Add(baseIndex); tris1.Add(baseIndex + 1); tris1.Add(baseIndex + 2);
            }
            else
            {
                int baseIndex = verts2.Count;
                verts2.Add(v0); verts2.Add(v1); verts2.Add(v2);
                tris2.Add(baseIndex); tris2.Add(baseIndex + 1); tris2.Add(baseIndex + 2);
            }
        }

        mesh1.SetVertices(verts1);
        mesh1.SetTriangles(tris1, 0);
        mesh1.RecalculateNormals();

        mesh2.SetVertices(verts2);
        mesh2.SetTriangles(tris2, 0);
        mesh2.RecalculateNormals();

        // Ajout des MeshFilter et MeshRenderer
        MeshFilter mf1 = child1.AddComponent<MeshFilter>();
        MeshRenderer mr1 = child1.AddComponent<MeshRenderer>();
        MeshCollider mh1 = child1.AddComponent<MeshCollider>();
        mh1.sharedMesh = mesh1;
        mf1.sharedMesh = mesh1;
        mr1.material = GetComponent<MeshRenderer>().sharedMaterial;

        MeshFilter mf2 = child2.AddComponent<MeshFilter>();
        MeshRenderer mr2 = child2.AddComponent<MeshRenderer>();
        MeshCollider mh2 = child2.AddComponent<MeshCollider>();
        mh2.sharedMesh = mesh2;
        mf2.sharedMesh = mesh2;
        mr2.material = GetComponent<MeshRenderer>().sharedMaterial;

        // Supprimer le MeshRenderer et MeshFilter du parent
        DestroyImmediate(GetComponent<MeshFilter>());
        DestroyImmediate(GetComponent<MeshRenderer>());
        DestroyImmediate(GetComponent<MeshCollider>());
        
    }

    // Pour tester depuis l'éditeur
    [ContextMenu("Split Mesh")]
    void SplitMeshEditor()
    {
        SplitMesh();
    }
}
