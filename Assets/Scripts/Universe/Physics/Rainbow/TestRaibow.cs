using UnityEngine;


public class TestRaibow : MonoBehaviour
{
    [Header("param")]
    public float H;

    [Header("rayon d'incidence")]
    Vector3 LightDirection;
    
    Vector3 originLight;
    public float maxDistance;
    Vector3 viewDirection;

    [Header("joueur")]
    public GameObject joueur;
    public GameObject mylight;
    


    Renderer mesh;
    Color originalcolor;

    private void Start()
    {
        mesh = GetComponent<Renderer>();
        originalcolor = mesh.material.color; 

    
    }
    
    private void Update()
    {

        LightDirection = mylight.transform.forward;
        viewDirection = joueur.transform.forward;
        originLight = mylight.transform.position;

        float h = Mathf.Clamp(Vector3.Dot(LightDirection, viewDirection), -1f, 1f);
        H = Mathf.Acos(h) * Mathf.Rad2Deg;

        Debug.DrawRay(originLight, LightDirection * maxDistance);
        Debug.DrawRay(originLight, viewDirection * maxDistance, Color.red);
        
        float wavelength = Mathf.Lerp(400f, 650f, Mathf.InverseLerp(40.5f, 43.0f, H));
        Color color = H < 43.5f && H > 40.0f ? WavelengthToRGB_DanBruton(wavelength) : originalcolor;


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

        // FACTOR = correction de luminosit�
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
