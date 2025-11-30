using UnityEngine;


public class TestRaibow : MonoBehaviour
{
    public float i;
    public float n;
    public float r;
    public float D;

    public GameObject regard;

    [Header("rayon d'incidence")]
    public Vector3 originLight;
    public Vector3 direction;
    public float maxDistance;

    Renderer mesh;

    private void Start()
    {
        mesh = GetComponent<Renderer>();
    }

    private void Update()
    {
        Vector3 origin = transform.position - originLight;
        Vector3 viewDir = (regard.transform.position - transform.position).normalized;
        float viewAngle = Vector3.Angle(viewDir, -direction);  // en degrés
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;
        Debug.DrawRay(origin, direction * maxDistance);
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            //si on touche
            i = Mathf.Acos(Vector3.Dot(-direction, hit.normal));
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }
         
        direction = direction.normalized;

        r = Mathf.Asin(Mathf.Sin(i) / n);
        D = Mathf.Abs((Mathf.PI - (4*r - 2*i)) * Mathf.Rad2Deg);

        float wavelength = Mathf.Lerp(400f, 650f, Mathf.InverseLerp(40.5f, 43.0f, D));
        Color color = WavelengthToRGB_DanBruton(wavelength);


        mesh.material.color = color; 

    }

    public static Color WavelengthToRGB_DanBruton(float wavelength)
    {
        float R = 0f, G = 0f, B = 0f;
        float factor;

        if (wavelength >= 380 && wavelength <= 440)
        {
            R = -(wavelength - 440f) / (440f - 380f);
            G = 0f;
            B = 1f;
        }
        else if (wavelength > 440 && wavelength <= 490)
        {
            R = 0f;
            G = (wavelength - 440f) / (490f - 440f);
            B = 1f;
        }
        else if (wavelength > 490 && wavelength <= 510)
        {
            R = 0f;
            G = 1f;
            B = -(wavelength - 510f) / (510f - 490f);
        }
        else if (wavelength > 510 && wavelength <= 580)
        {
            R = (wavelength - 510f) / (580f - 510f);
            G = 1f;
            B = 0f;
        }
        else if (wavelength > 580 && wavelength <= 645)
        {
            R = 1f;
            G = -(wavelength - 645f) / (645f - 580f);
            B = 0f;
        }
        else if (wavelength > 645 && wavelength <= 780)
        {
            R = 1f;
            G = 0f;
            B = 0f;
        }

        // FACTOR = correction de luminosité
        if (wavelength >= 380 && wavelength < 420)
        {
            factor = 0.3f + 0.7f * (wavelength - 380f) / (420f - 380f);
        }
        else if (wavelength >= 420 && wavelength <= 700)
        {
            factor = 1f;
        }
        else if (wavelength > 700 && wavelength <= 780)
        {
            factor = 0.3f + 0.7f * (780f - wavelength) / (780f - 700f);
        }
        else
        {
            factor = 0f;
        }

        R = Mathf.Clamp01(R * factor);
        G = Mathf.Clamp01(G * factor);
        B = Mathf.Clamp01(B * factor);

        return new Color(R, G, B);
    }

}
