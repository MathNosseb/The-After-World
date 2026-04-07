using UnityEngine;

[ExecuteInEditMode]
public class EclipseGenerator : MonoBehaviour
{
    CelestialBody sun;
    public PlayerContainer playerContainer;
    public Light sunLight;
    Camera cam;

    [Range(1,30)]
    public int nbr_points;

    Vector3[] positions;
    bool init = false;

    [ContextMenu("Generate Dots")]
    public void GenerateDots()
    {
        if (sun == null)
            sun = GameObject.Find("Sun").GetComponent<CelestialBody>();
        if (playerContainer == null)
            playerContainer = FindFirstObjectByType<PlayerContainer>();
        if (cam == null)
            cam = FindFirstObjectByType<Camera>();
        if (sunLight == null)
            sunLight = GetComponentInChildren<Light>();
        init = false;
        positions = new Vector3[nbr_points];
        for (int i = 0; i < nbr_points; i++)
        {
            Vector3 center = sun.GetVector3Position();
            Vector3 toPlayer = (playerContainer.transform.position - center).normalized;

            Vector3 forward = toPlayer;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right);

            float radius = sun.diametre / 2f;
            float angle = (2f * Mathf.PI / nbr_points) * i;

            Vector3 pos = center 
                + right * Mathf.Cos(angle) * radius 
                + up * Mathf.Sin(angle) * radius;

            positions[i] = pos;
        }
        init = true;
    }


    void Update()
    {

        GenerateDots();
        int nbr = 0;
        Vector3 sunPos = sun.GetVector3Position();
        
        
        //on tire des rayons vers le joueur
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 dir = (positions[i] - cam.transform.position).normalized;
            float dst = Vector3.Distance(positions[i],cam.transform.position);
            Ray ray = new Ray(cam.transform.position, dir);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, dst) && hit.collider.gameObject != playerContainer.gameObject)
            {
                GameObject root = hit.collider.transform.root.gameObject;
                CelestialBody celestial = root.GetComponent<CelestialBody>();

                if (celestial == null)
                {
                    //pas une planete
                    
                    Debug.DrawRay(cam.transform.position,dir * dst, Color.green);
                }
                else
                {
                    if (root == sun.gameObject)
                    {
                        Debug.DrawRay(cam.transform.position,dir * dst, Color.green);
                    }
                    else
                    {
                        nbr+=1;
                        Debug.DrawRay(cam.transform.position,dir * dst, Color.red);
                    }
                }

                
            }
            else
            {
                Debug.DrawRay(cam.transform.position,dir * dst, Color.green);
            }

            
        }

        float visibility = 1f - ((float)nbr / nbr_points);
        sunLight.intensity = visibility;


    }

    void OnDrawGizmos()
    {
        if (!init) return;

        for (int i = 0; i < positions.Length; i++)
        {
            Gizmos.DrawSphere(positions[i], 10);
        }
    }
}
