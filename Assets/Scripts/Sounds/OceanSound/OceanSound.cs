using UnityEngine;

[ExecuteInEditMode]
public class OceanSound : MonoBehaviour
{
    Ray[] rays;
    RaycastHit[] hits;

    Vector3[] directions;

    public void Init()
    {
        directions = new Vector3[8];
        directions[0] = Vector3.right;
        directions[1] = (Vector3.right + Vector3.forward).normalized;
        directions[2] = Vector3.forward;
        directions[3] = (Vector3.forward + Vector3.left).normalized;
        directions[4] = Vector3.left;
        directions[5] = (Vector3.left + Vector3.back).normalized;
        directions[6] = Vector3.back;
        directions[7] = (Vector3.back + Vector3.right).normalized;

        rays = new Ray[8];
        hits = new RaycastHit[8];
    }

    private void Update()
    {
        Init();
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 dir = transform.rotation * directions[i];
            rays[i] = new Ray((transform.position + dir * 10f) - (Vector3.up * 5f), Vector3.up * 10f);
            Debug.DrawRay(rays[i].origin, rays[i].direction, Color.green);

            if (Physics.Raycast(rays[i], out hits[i], 10f))
            {
                Debug.DrawRay((transform.position + directions[i] * 10f) - (Vector3.up * 5f), Vector3.up * 10f, Color.blue);
            }else

                Debug.DrawRay((transform.position + directions[i] * 10f) - (Vector3.up * 5f), Vector3.up * 10f, Color.red);

            Vector3 center = (transform.position + directions[i] * 10f) - (Vector3.up * 5f); // centre de la sphère
            float radius = 5f; // rayon de détection
            Collider[] hit = Physics.OverlapSphere(center, radius);

            foreach (Collider c in hit)
            {
                Debug.Log("Objet détecté : " + c.name);
                Vector3 closestPoint = c.ClosestPoint(center);
                Debug.DrawLine(center, closestPoint, Color.red);
            }
        }

        
    }
}
