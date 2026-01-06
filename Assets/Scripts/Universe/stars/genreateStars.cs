using System.Collections.Generic;
using UnityEngine;

public class genreateStars : MonoBehaviour
{
    private const int CircleSegmentCount = 64;
    private const int CircleVertexCount = CircleSegmentCount + 2;
    private const int CircleIndexCount = CircleSegmentCount * 3;

    Mesh mesh;
    Material mat;
    List<Vector3> positions;
    List<Quaternion> rotations;
    List<Matrix4x4> matrix4X4s;

    public int count = 10;
    public float radius = 15f;
    public int size;

    


    private void Start()
    {
        mesh = GenerateCircleMesh();
        mat = new Material(Shader.Find("Standard"));

        matrix4X4s = new List<Matrix4x4>();
        rotations = new List<Quaternion>();

        positions = GenerateSpherePositions(count, radius, Vector3.zero);
        
        Vector3 scale = Vector3.one * size;                  // scale (1,1,1)
        foreach (var position in positions)
        {
            Vector3 direction = Vector3.zero - position.normalized;
            rotations.Add(Quaternion.LookRotation(direction));
            matrix4X4s.Add(Matrix4x4.TRS(position, Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0), scale));
        }

        
    }

    private void Update()
    {
        foreach (var matrix4X4 in matrix4X4s)
        {
            Graphics.DrawMesh(mesh, matrix4X4, mat, 0);
        }
        
    }

    List<Vector3> GenerateSpherePositions(int count, float radius, Vector3 center)
    {
        List<Vector3> positions = new List<Vector3>();

        // méthode de Fibonacci pour répartir les points sur une sphère
        float offset = 2f / count;
        float increment = Mathf.PI * (3f - Mathf.Sqrt(5f)); // angle d'or

        for (int i = 0; i < count; i++)
        {
            float y = i * offset - 1f + (offset / 2f); // y entre -1 et 1
            float r = Mathf.Sqrt(1 - y * y);            // rayon du cercle à ce y
            float phi = i * increment;

            float x = Mathf.Cos(phi) * r;
            float z = Mathf.Sin(phi) * r;

            positions.Add(center + new Vector3(x, y, z) * radius);
        }

        return positions;
    }

    private static Mesh GenerateCircleMesh()
    {
        var circle = new Mesh();
        var vertices = new List<Vector3>(CircleVertexCount);
        var indices = new int[CircleIndexCount];
        var segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
        var angle = 0f;
        vertices.Add(Vector3.zero);
        for (int i = 1; i < CircleVertexCount; ++i)
        {
            vertices.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
            angle -= segmentWidth;
            if (i > 1)
            {
                var j = (i - 2) * 3;
                indices[j + 0] = 0;
                indices[j + 1] = i - 1;
                indices[j + 2] = i;
            }
        }
        circle.SetVertices(vertices);
        circle.SetIndices(indices, MeshTopology.Triangles, 0);
        circle.RecalculateBounds();
        return circle;
    }
}
