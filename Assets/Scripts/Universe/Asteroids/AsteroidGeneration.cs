using Unity.VisualScripting;
using UnityEngine;


[ExecuteInEditMode]
public class AsteroidGeneration : MonoBehaviour
{
    public AsteroidData asteroidData;
    bool initialized = false;

    void OnValidate()
    {
        initialized = false;
        Debug.Log("changement");
        Init();
    }


    void Init()
    {
        asteroidData.kernel = asteroidData.computeShader.FindKernel("CSMain");
        for (int asteroidDataIndex = 0; asteroidDataIndex < asteroidData.asteroidContainer.Length; asteroidDataIndex++)
        {
            asteroidData.asteroidContainer[asteroidDataIndex].bufferArgs?.Release();
            asteroidData.asteroidContainer[asteroidDataIndex].bufferPositions?.Release();
            asteroidData.asteroidContainer[asteroidDataIndex].outputPositions?.Dispose();
            asteroidData.asteroidContainer[asteroidDataIndex].bufferArgs = new ComputeBuffer(
                1,
                5*sizeof(int),
                ComputeBufferType.IndirectArguments
            );

            uint[] args = new uint[5]
            {
                asteroidData.asteroidContainer[asteroidDataIndex].mesh.GetIndexCount(0),
                (uint)asteroidData.asteroidContainer[asteroidDataIndex].count,
                asteroidData.asteroidContainer[asteroidDataIndex].mesh.GetIndexStart(0),
                asteroidData.asteroidContainer[asteroidDataIndex].mesh.GetBaseVertex(0),
                0
            };

            asteroidData.asteroidContainer[asteroidDataIndex].bufferArgs.SetData(args);

            asteroidData.asteroidContainer[asteroidDataIndex].bufferPositions = new ComputeBuffer(
                asteroidData.asteroidContainer[asteroidDataIndex].count,
                3*sizeof(float)
            );

            int count = asteroidData.asteroidContainer[asteroidDataIndex].count;

            Vector3[] positions = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                positions[i] = new Vector3(i*2,0,0);
            }
            asteroidData.asteroidContainer[asteroidDataIndex].outputPositions = new ComputeBuffer(
                asteroidData.asteroidContainer[asteroidDataIndex].count, 
                sizeof(float) * 3, 
                ComputeBufferType.Append
            );

            asteroidData.asteroidContainer[asteroidDataIndex].bufferPositions.SetData(positions);
        }
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;
        Bounds bounds = new Bounds(transform.position, new Vector3(1000000,1000000,10000));
        for (int asteroidDataIndex = 0; asteroidDataIndex < asteroidData.asteroidContainer.Length; asteroidDataIndex++)
        {
            asteroidData.asteroidContainer[asteroidDataIndex].outputPositions.SetCounterValue(0);

            asteroidData.asteroidContainer[asteroidDataIndex].material.SetBuffer("_AsteroidsPosition",
                                    asteroidData.asteroidContainer[asteroidDataIndex].outputPositions );
            asteroidData.asteroidContainer[asteroidDataIndex].material.SetColor("_Color", 
                                    asteroidData.asteroidContainer[asteroidDataIndex].color);
            asteroidData.asteroidContainer[asteroidDataIndex].material.SetMatrix("_ObjectToWorld", 
                                    transform.localToWorldMatrix); 

            asteroidData.computeShader.SetBuffer(asteroidData.kernel, "inputPositions", 
                                                asteroidData.asteroidContainer[asteroidDataIndex].bufferPositions);
            asteroidData.computeShader.SetBuffer(asteroidData.kernel, "outputPositions", 
                                                asteroidData.asteroidContainer[asteroidDataIndex].outputPositions);
            asteroidData.computeShader.SetVector("center", transform.position);
            asteroidData.computeShader.SetInt("count", asteroidData.asteroidContainer[asteroidDataIndex].count);
            asteroidData.computeShader.SetFloat("radius", asteroidData.asteroidContainer[asteroidDataIndex].radius);

            int groups = Mathf.CeilToInt(asteroidData.asteroidContainer[asteroidDataIndex].count / 64f);
            asteroidData.computeShader.Dispatch(asteroidData.kernel, groups, 1, 1);

            ComputeBuffer.CopyCount(asteroidData.asteroidContainer[asteroidDataIndex].outputPositions, 
                        asteroidData.asteroidContainer[asteroidDataIndex].bufferArgs, sizeof(uint));


            Graphics.DrawMeshInstancedIndirect(asteroidData.asteroidContainer[asteroidDataIndex].mesh,
                                                 0,
                                                  asteroidData.asteroidContainer[asteroidDataIndex].material,
                                                 bounds,
                                                 asteroidData.asteroidContainer[asteroidDataIndex].bufferArgs
                                                 );
                        

            
        }
        
    }

    void OnDisable()
    {
        for (int asteroidDataIndex = 0; asteroidDataIndex < asteroidData.asteroidContainer.Length; asteroidDataIndex++)
        {
            asteroidData.asteroidContainer[asteroidDataIndex].bufferArgs?.Dispose();
            asteroidData.asteroidContainer[asteroidDataIndex].bufferPositions?.Dispose();
            asteroidData.asteroidContainer[asteroidDataIndex].outputPositions?.Dispose();
        }
            
    }
}
