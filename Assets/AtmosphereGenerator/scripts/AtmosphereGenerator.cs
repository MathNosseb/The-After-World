using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[CreateAssetMenu(menuName = "Custom Post Effects/Atmosphere")]     
public class AtmosphereGenerator : PostProcessingEffect
{
    public Shader shader;
    public ComputeShader opticalDepthCompute;
    public Vector3 planetCentre;
    public float atmosphereRadius; 
    public float planetRadius;
    public float densityFalloff; 
    public float numOpticalDepthPoints; 
    public float numInScatteringPoints;
    public Vector3 waveLengths = new Vector3(700, 530, 440);
    public float scatteringStrength = 1;
    public Vector3 lightDir;


    public int textureSize = 256;
    public Texture2D blueNoise;
    RenderTexture opticalDepthTexture;
    public float ditherStrength = 0.8f;
    public float ditherScale = 4;
    public float intensity = 1;
    bool settingsUpToDate;

    void OnEnable() 
    { 
        Camera cam = Camera.current;
        if (cam != null) 
            cam.depthTextureMode |= DepthTextureMode.Depth;
         
        if (shader == null)
            shader = Shader.Find("Custom/PostProcessRaymarchWorld"); 
        material = new Material(shader);
    }
     
    public override void Render(RenderTexture source, RenderTexture destination) 
    {
        if (material == null)
        { 
            Graphics.Blit(source, destination);
            return;
        }
         
        Camera cam = Camera.current;
        float scatterR = Mathf.Pow(400 / waveLengths.x, 4) * scatteringStrength; 
        float scatterG = Mathf.Pow(400 / waveLengths.y, 4) * scatteringStrength;
        float scatterB = Mathf.Pow(400 / waveLengths.z, 4) * scatteringStrength;
        Vector3 scaterringCoefficients = new Vector3(scatterR, scatterG, scatterB);


        material.SetVector("_scaterringCoefficients", scaterringCoefficients);
        material.SetVector("_planetCentre", planetCentre);
        material.SetVector("_dirToSun", lightDir);
        material.SetFloat("_atmosphereRadius", atmosphereRadius);
        material.SetFloat("_planetRadius", planetRadius);
        material.SetFloat("_densityFalloff", densityFalloff);
        material.SetFloat("_numOpticalDepthPoints", numOpticalDepthPoints);
        material.SetFloat("_numInScatteringPoints", numInScatteringPoints);
        material.SetFloat("intensity", intensity);
        material.SetFloat("_ditherStrength", ditherStrength);
        material.SetFloat("_ditherScale", ditherScale);
        material.SetTexture("_BlueNoise", blueNoise);

        PrecomputeOutScattering(); 
        material.SetTexture("_BakedOpticalDepth", opticalDepthTexture);
         
        settingsUpToDate = true;


        Graphics.Blit(source, destination, material);
    }
     
    void PrecomputeOutScattering()
    {
        if (!settingsUpToDate || opticalDepthTexture == null || !opticalDepthTexture.IsCreated())
        {
            ComputeHelper.CreateRenderTexture(ref opticalDepthTexture, textureSize, FilterMode.Bilinear);
            opticalDepthCompute.SetTexture(0, "Result", opticalDepthTexture);
            opticalDepthCompute.SetInt("textureSize", textureSize);
            opticalDepthCompute.SetInt("numOutScatteringSteps", (int)numOpticalDepthPoints);
            opticalDepthCompute.SetFloat("atmosphereRadius", (1 + atmosphereRadius));
            opticalDepthCompute.SetFloat("densityFalloff", densityFalloff);//
            //planetRadius
            opticalDepthCompute.SetFloat("planetRadius", planetRadius); 
            ComputeHelper.Run(opticalDepthCompute, textureSize, textureSize);
        }

    }

    void OnValidate()
    {
        settingsUpToDate = false;
    }
}
