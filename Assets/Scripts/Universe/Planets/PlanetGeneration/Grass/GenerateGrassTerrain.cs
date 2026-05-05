using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;


public class GenerateGrassTerrain : MonoBehaviour
{
    Grass grass;

    public void SetUpGrass(int face, Grass grassCompo, GrassMeshData grassMeshData)
    {
        grass = grassCompo;
        grass.cmd = new CommandBuffer();

        FindFirstObjectByType<Camera>().AddCommandBuffer(grass.cameraEvent, grass.cmd);
        grass.positionsBuffer[face]?.Release();
        grass.rotationBuffer[face]?.Release();
        grass.argsBuffer[face]?.Release();
        grass.noiseBuffer[face]?.Release();

        Mesh surfaceMesh = grass.surface[face].GetComponent<MeshFilter>().sharedMesh;
        Debug.Log("[INFORMATION] Création de " + grass.density * surfaceMesh.vertexCount + " grass Mesh");
        Vector3[] vertsMesh = surfaceMesh.vertices; // une seule alloc
        Vector2[] uvsMesh = surfaceMesh.uv;
        Vector3[] normalsMesh = surfaceMesh.normals;

        NativeArray<Vector3> verts = new NativeArray<Vector3>(surfaceMesh.vertexCount, Allocator.TempJob);
        NativeArray<Vector2> uvs = new NativeArray<Vector2>(surfaceMesh.uv.Length, Allocator.TempJob);
        NativeArray<Vector3> normals = new NativeArray<Vector3>(surfaceMesh.normals.Length, Allocator.TempJob);
        verts.CopyFrom(vertsMesh);
        normals.CopyFrom(normalsMesh);
        uvs.CopyFrom(uvsMesh);

        var job = new InitGrassJob
        {
            density = grass.density,
            spread = grass.spread,
            verts = verts,
            normals = normals,
            uvs = uvs,
            positions = new NativeArray<Vector3>(surfaceMesh.vertexCount * grass.density, Allocator.TempJob),
            rotations = new NativeArray<quaternion>(surfaceMesh.vertexCount * grass.density, Allocator.TempJob),
            noises = new NativeArray<float>(surfaceMesh.vertexCount * grass.density, Allocator.TempJob),
            valid = new NativeArray<bool>(surfaceMesh.vertexCount * grass.density, Allocator.TempJob),
            minDistance = grass.minDistance,
            maxDistance = grass.maxDistance + grass.minDistance,
            planetPosition = transform.position,
            maxAngleTerrain = grass.maxAngleTerrain,
            continentNoise = grass.continentNoise,
            warpNoise = grass.warpNoise,
            mountainNoise = grass.mountainNoise,
            mountainMaskNoise = grass.mountainMaskNoise,
            planetRadius = grass.planetRadius,
            seed = grass.seed,
            PlanetFacedirection = grass.directions[face]
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

        grass.bladeCounts[face] = finalPosition.Count;

        grass.grassMesh = grassMeshData.GetCustomMesh();

        grass.positionsBuffer[face] = new ComputeBuffer(finalPosition.Count, sizeof(float) * 3);
        grass.positionsBuffer[face].SetData(finalPosition);
        grass.rotationBuffer[face] = new ComputeBuffer(finalRotation.Count, sizeof(float) * 4);
        grass.rotationBuffer[face].SetData(finalRotation);
        grass.noiseBuffer[face] = new ComputeBuffer(finalNoise.Count, sizeof(float));
        grass.noiseBuffer[face].SetData(finalNoise);


        grass.mat[face].SetBuffer("_Positions", grass.positionsBuffer[face]);
        grass.mat[face].SetBuffer("_Rotations", grass.rotationBuffer[face]);
        grass.mat[face].SetColor("_ColorBase", grass.BaseColor);
        grass.mat[face].SetColor("_ColorTip", grass.TopColor);
        grass.mat[face].SetBuffer("_Noises", grass.noiseBuffer[face]);
        grass.sun = GameObject.Find("Sun");
        
        grass.sunCelestial = grass.sun.GetComponent<CelestialBody>();
        Vector3 lightDir = (grass.sunCelestial.GetDoubleVector3Position().convert - transform.position).normalized;
        grass.mat[face].SetVector("_dirToSun", lightDir);

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

        grass.outputBladeData[face] = new ComputeBuffer(grass.bladeCounts[face], 
                                                                sizeof(float) * 3 +  sizeof(float) * 4 + sizeof(float), 
                                                                ComputeBufferType.Append);

        grass.faceInit[face] = true;
    }

    void Update()
    { 
        Camera camera;
        camera = FindFirstObjectByType<Camera>();
        grass.cmd.Clear();
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera); 
        for (int i = 0; i < 6; i++)
        {
            if (!grass.faceInit[i]) continue;
            if (!GeometryUtility.TestPlanesAABB(planes, grass.meshRenderers[i].bounds)) continue; //pas visible a l ecran
            if (grass.outputBladeData[i] == null)
            {
                continue;
                //grass.outputBladeData = new ComputeBuffer[6];
            
            }
            grass.outputBladeData[i].SetCounterValue(0);
            
            grass.computeShader.SetBuffer(grass.kernel, "inputPositions", grass.positionsBuffer[i]);
            grass.computeShader.SetBuffer(grass.kernel, "inputRotations", grass.rotationBuffer[i]);
            grass.computeShader.SetBuffer(grass.kernel, "inputNoises", grass.noiseBuffer[i]);
            grass.computeShader.SetBuffer(grass.kernel, "outputBlades", grass.outputBladeData[i]);
            Vector3 localPlayerPos = grass.surface[i].transform.InverseTransformPoint(
                camera.transform.position
            );

            Vector3 localCamForward = grass.surface[i].transform.InverseTransformDirection(
                camera.transform.forward
            );
            grass.computeShader.SetVector("_PlayerPos", localPlayerPos);
            grass.computeShader.SetFloat("_MaxDistance", grass.viewDistance);
            grass.computeShader.SetInt("_BladeCount", grass.bladeCounts[i]);
            grass.computeShader.SetVector("_CamForward", localCamForward);
            grass.computeShader.SetFloat("_Fov", camera.fieldOfView);
            grass.computeShader.SetFloat("_Crop", grass.cropEffect);

            int groups = Mathf.CeilToInt(grass.bladeCounts[i] / 64f);
            grass.computeShader.Dispatch(grass.kernel, groups, 1, 1);

            ComputeBuffer.CopyCount(grass.outputBladeData[i], grass.argsBuffer[i], sizeof(uint));

            Vector3 lightDir = (grass.sunCelestial.GetDoubleVector3Position().convert - transform.position).normalized;
            grass.mat[i].SetVector("_dirToSun", lightDir);
            grass.mat[i].SetMatrix("_ObjectToWorld", grass.surface[i].transform.localToWorldMatrix); 
            grass.mat[i].SetVector("playerPosition", localPlayerPos);
            grass.mat[i].SetBuffer("_Blades", grass.outputBladeData[i]);

            grass.cmd.DrawMeshInstancedIndirect(grass.grassMesh, 0, grass.mat[i], 0, grass.argsBuffer[i]);

            
        }
        


    }

    void OnDestroy()
    {
        for (int i = 0; i < 6; i++)
        {
            grass.positionsBuffer[i]?.Release();
            grass.rotationBuffer[i]?.Release();
            grass.argsBuffer[i]?.Release();
            grass.noiseBuffer[i]?.Release();
            grass.outputBladeData[i]?.Release();
        }

        
    }

}
