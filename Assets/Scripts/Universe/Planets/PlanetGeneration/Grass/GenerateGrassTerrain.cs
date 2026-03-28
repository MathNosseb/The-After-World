using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


public class GenerateGrassTerrain : MonoBehaviour
{
    Grass grass;

    public void SetUpGrass(int face, Grass grassCompo)
    {
        grass = grassCompo;
        grass.cmd = new CommandBuffer();
        CameraManager.cam.AddCommandBuffer(grass.cameraEvent, grass.cmd);
        
        grass.positionsBuffer[face]?.Release();
        grass.rotationBuffer[face]?.Release();
        grass.argsBuffer[face]?.Release();
        grass.noiseBuffer[face]?.Release();

        Transform surfaceTransform = grass.surface[face].GetComponent<Transform>();
        Mesh surfaceMesh = grass.surface[face].GetComponent<MeshFilter>().sharedMesh;
        Debug.Log("[INFORMATION] Création de " + grass.density * surfaceMesh.vertexCount + " grass Mesh");
        Vector3[] vertsMesh = surfaceMesh.vertices; // une seule alloc
        Vector3[] normalsMesh = surfaceMesh.normals;

        NativeArray<Vector3> verts = new NativeArray<Vector3>(surfaceMesh.vertexCount, Allocator.TempJob);
        NativeArray<Vector3> normals = new NativeArray<Vector3>(surfaceMesh.normals.Length, Allocator.TempJob);
        verts.CopyFrom(vertsMesh);
        normals.CopyFrom(normalsMesh);

        var job = new InitGrassJob
        {
            density = grass.density,
            spread = grass.spread,
            verts = verts,
            normals = normals,
            positions = new NativeArray<Vector3>(surfaceMesh.vertexCount * grass.density, Allocator.TempJob),
            rotations = new NativeArray<quaternion>(surfaceMesh.vertexCount * grass.density, Allocator.TempJob),
            noises = new NativeArray<float>(surfaceMesh.vertexCount * grass.density, Allocator.TempJob),
            valid = new NativeArray<bool>(surfaceMesh.vertexCount * grass.density, Allocator.TempJob),
            minDistance = grass.minDistance,
            maxDistance = grass.maxDistance + grass.minDistance,
        };

        var handlejob = job.Schedule(surfaceMesh.vertexCount * grass.density, 64);
        handlejob.Complete();

        List<Vector3> finalPosition = new List<Vector3>();
        List<quaternion> finalRotation = new List<quaternion>();
        List<float> finalNoise = new List<float>();
        for (int i = 0; i < surfaceMesh.vertexCount * grass.density; i++)
        {
            if (job.valid[i])
            {
                finalPosition.Add(job.positions[i]);
                finalRotation.Add(job.rotations[i]);
                finalNoise.Add(job.noises[i]);
            }
        }

        if (finalPosition.Count == 0) {
            verts.Dispose();
            normals.Dispose();
            job.positions.Dispose();
            job.rotations.Dispose();
            job.noises.Dispose();
            job.valid.Dispose();
            return;
        }

        grass.grassMesh = GetComponent<GrassMesh>().GetGrassMesh();

        grass.positionsBuffer[face] = new ComputeBuffer(finalPosition.Count, sizeof(float) * 3);
        grass.positionsBuffer[face].SetData(finalPosition);
        grass.rotationBuffer[face] = new ComputeBuffer(finalRotation.Count, sizeof(float) * 4);
        grass.rotationBuffer[face].SetData(finalRotation);
        grass.noiseBuffer[face] = new ComputeBuffer(finalNoise.Count, sizeof(float));
        grass.noiseBuffer[face].SetData(finalNoise);


        grass.mat.SetBuffer("_Positions", grass.positionsBuffer[face]);
        grass.mat.SetBuffer("_Rotations", grass.rotationBuffer[face]);
        grass.mat.SetColor("_Color", grass.color);
        grass.mat.SetBuffer("_Noises", grass.noiseBuffer[face]);
        grass.sun = GameObject.Find("Sun");
        
        grass.sunCelestial = grass.sun.GetComponent<CelestialBody>();
        Vector3 lightDir = (grass.sunCelestial.GetDoubleVector3Position().convert - transform.position).normalized;
        grass.mat.SetVector("_dirToSun", lightDir);

        grass.argsBuffer[face] = new ComputeBuffer(
            1,
            5*sizeof(int),
            ComputeBufferType.IndirectArguments
        );

        uint[] args = new uint[5]
        {
            grass.grassMesh.GetIndexCount(0),//nombre de triangles dans le mesh
            (uint)finalPosition.Count ,//nombre d instance a dessiner
            grass.grassMesh.GetIndexStart(0),
            grass.grassMesh.GetBaseVertex(0),
            0,

        };

        verts.Dispose();
        normals.Dispose();
        job.positions.Dispose();
        job.rotations.Dispose();
        job.noises.Dispose();
        job.valid.Dispose();

        grass.argsBuffer[face].SetData(args);

        grass.faceInit[face] = true;
    }

    void Update()
    {
        for (int i = 0; i < 6; i++)
        {
            if (!grass.faceInit[i]) return;
            if (grass.argsBuffer[i] == null || grass.grassMesh == null ) return;
            Vector3 lightDir = (grass.sunCelestial.GetDoubleVector3Position().convert - transform.position).normalized;
            grass.mat.SetVector("_dirToSun", lightDir);
            grass.mat.SetMatrix("_ObjectToWorld", grass.surface[i].transform.localToWorldMatrix); 

            grass.cmd.Clear();
            grass.cmd.DrawMeshInstancedIndirect(grass.grassMesh, 0, grass.mat, 0, grass.argsBuffer[i]);
        }
        


    }

}
