using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;


[BurstCompile]
public struct InitGrassJob : IJobParallelFor
{
    public int density;
    public float spread;
    
    public float minDistance;
    public float maxDistance;

    public float3 planetPosition;
    public float maxAngleTerrain;


    [ReadOnly] public NativeArray<Vector3> verts;
    [ReadOnly] public NativeArray<Vector3> normals;
    public NativeArray<Vector3> positions;
    public NativeArray<quaternion> rotations;
    public NativeArray<float> noises;
    public NativeArray<bool> valid;
    

    public void Execute(int index)
    {
        int i = index / density;

        float3 worldPos = verts[i];

        Vector3 N = normals[i];
        Vector3 arbitrary = (Mathf.Abs(N.y) < 0.99f) ? Vector3.up : Vector3.right;
        float3 T = math.normalize(math.cross(N, arbitrary));
        float3 B = math.cross(N, T);
        Unity.Mathematics.Random rand = new Unity.Mathematics.Random((uint)math.hash(new uint2((uint)index, 1337)));

        float angle = rand.NextFloat(0f, math.PI * 2f);
        float radius = math.sqrt(rand.NextFloat());

        float2 p = new float2(
            math.cos(angle) * radius,
            math.sin(angle) * radius
        );
        
        Vector2 randPos = p * spread;
        float3 offset = T * randPos.x + B * randPos.y;

        float3 position = worldPos + offset;
        float distance = math.length(position);

        float3 planetUp = math.normalize(position);
        float angleFromNormal = math.dot(N, planetUp);

        if (distance <= maxDistance && distance >= minDistance && angleFromNormal >= maxAngleTerrain)
        {
            positions[index] = worldPos + offset;
        
            noises[index] = Unity.Mathematics.noise.pnoise(worldPos.xy, new float2(100f,100f));

            float3 up = math.normalize(position);

            // rotation qui aligne Y → up
            quaternion align = FromToRotation(new float3(0,1,0), up);

            // rotation aléatoire autour de la normale
            float randomAngle = rand.NextFloat(0f, math.PI * 2f);
            quaternion randomRot = quaternion.AxisAngle(up, randomAngle);

            // combinaison
            Quaternion finalRot = math.mul(randomRot, align);

            rotations[index] = finalRot;
            valid[index] = true;
        }
        else
            valid[index] = false;

        
    }

    public quaternion FromToRotation(float3 from, float3 to)
    {
        return quaternion.AxisAngle(
        angle: math.acos( math.clamp(math.dot(math.normalize(from),math.normalize(to)),-1f,1f) ) ,
        axis: math.normalize( math.cross(from,to) ));
    }
}
