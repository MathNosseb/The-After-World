using UnityEngine;

public class GrassMesh : MonoBehaviour
{
    Vector3[] vertices;
    int[] triangles;
    Mesh mesh;

 
    [Range(0f,1f)]
    public float hauteur = 1f;

    [Range(0f, 1f)]
    public float longueur = 1f;
    public void GenerateMesh()
    {
        mesh = new Mesh
        {
            name = "grass"
        };

        vertices = new Vector3[]
        {
            new Vector3 (0,0),
            new Vector3 (0,1 * hauteur),
            new Vector3 (1*longueur,1 * hauteur),
            new Vector3 (1*longueur,0),
            new Vector3 (0.2f*longueur, 2 * hauteur),
            new Vector3 (0.8f*longueur, 2 * hauteur),
            new Vector3 (0.5f*longueur, 3 * hauteur),
        };

        triangles = new int[]
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

    }


    public Mesh GetGrassMesh()
    {
        GenerateMesh();
        return mesh;
    }
}
