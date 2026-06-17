using UnityEngine;

[CreateAssetMenu(fileName = "GrassMeshData", menuName = "PlanetGeneration/Grass Mesh Data")]
public class GrassMeshData : ScriptableObject
{
    [Range(0f,1f)]
    public float hauteur = 1f;

    [Range(0f,1f)]
    public float longueur = 1f;

    public bool customMeshCondition;
    public Mesh customMesh;


    public Mesh GetCustomMesh()
    {
        if (customMeshCondition) 
            return customMesh;
        return null;
    }

    public Mesh GenerateMesh()
    {

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0,0),
            new Vector3(0,1 * hauteur),
            new Vector3(1 * longueur,1 * hauteur),
            new Vector3(1 * longueur,0),
            new Vector3(0.2f * longueur, 2 * hauteur),
            new Vector3(0.8f * longueur, 2 * hauteur),
            new Vector3(0.5f * longueur, 3 * hauteur),
        };

        int[] triangles = new int[]
        {
            0,1,2,
            2,3,0,
            1,4,5,
            5,2,1,
            4,6,5
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}