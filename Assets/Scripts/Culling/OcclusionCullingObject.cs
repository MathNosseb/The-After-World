using UnityEngine;

public class OcclusionCullingObject : MonoBehaviour
{

    [HideInInspector] public Vector3[] corners;

    [ContextMenu("generate")]
    public void GeneratePoints()
    {
        //place des points au 4 coins des objets

        //determiner le centre du gameObject
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null ) { return; } // pas de renderer : impossible de determiner le centre
        Vector3 center = renderer.bounds.center;

        //determiner le bas et haut
        Vector3 min = renderer.bounds.min;
        Vector3 max = renderer.bounds.max;

        corners = new Vector3[8];

        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(max.x, min.y, min.z);
        corners[2] = new Vector3(min.x, max.y, min.z);
        corners[3] = new Vector3(max.x, max.y, min.z);

        corners[4] = new Vector3(min.x, min.y, max.z);
        corners[5] = new Vector3(max.x, min.y, max.z);
        corners[6] = new Vector3(min.x, max.y, max.z);
        corners[7] = new Vector3(max.x, max.y, max.z);

        //on supprime les précédents enfants
        for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
        {
            DestroyImmediate(transform.GetChild(childIndex).gameObject);
        }

        for (int cornerIndex = 0; cornerIndex < corners.Length; cornerIndex++)
        {
            GameObject child = new GameObject("Occlusion " + cornerIndex);
            child.transform.parent = transform;
            child.transform.position = corners[cornerIndex];
        }   

    }
}
