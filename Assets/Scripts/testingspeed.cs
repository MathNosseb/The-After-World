using UnityEngine;
using System.Diagnostics;


public class testingspeed : MonoBehaviour
{
    public ComputeShader compute;
    int[] ints = new int[10000]; 
    void Start()
    {
        Stopwatch swCPU = new Stopwatch();
        swCPU.Start();
        CPU();
        swCPU.Stop();
        UnityEngine.Debug.Log("TempsCPU : " + swCPU.ElapsedMilliseconds + " ms");

        Stopwatch swGPU = new Stopwatch();

    }

    public void CPU()
    {
        for (int i = 0; i < ints.Length; i++)
        {
            ints[i] = i;
        }
    }

    public void GPU()
    {

    }
}
