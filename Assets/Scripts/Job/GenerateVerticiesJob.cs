using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;


[BurstCompile]
public struct GenerateVerticiesJob : IJobParallelFor
{
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector2> uvs;
    [ReadOnly] public NativeArray<Vector3> directions;
    public Vector3 craterCenter;

    public int presetQuality;
    public int face;
    public float radius;
    public int moon;

    public FastNoiseLite continentNoise;
    public FastNoiseLite warpNoise;
    public FastNoiseLite mountainNoise;
    public FastNoiseLite mountainMaskNoise;

    //shapeSettings
    public float oceanScale;
    public float noiseScale1;
    public float noiseScale2;
    public void Execute(int index)
    {
        int y = index / (presetQuality + 1);
        int x = index % (presetQuality + 1);

        Vector3 localUp = directions[face];
        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);

        Vector2 percent = new Vector2(x,y) / presetQuality;

        uvs[index] = percent;

        Vector3 point = localUp +
                        (percent.x - 0.5f) * 2f * axisA +
                        (percent.y - 0.5f) * 2f * axisB;
        
        point = point.normalized;

        if (moon == 0)
        {
            float noise = GetPlanetHeight(point);
            vertices[index] = point * (radius + noise);
        }
        else
        {
            //on créer la surface d une lune
            //la surface doit avoir un trou
            float craterRadius = 0.3f; // ajuste selon la taille voulue
            float distance = math.acos(math.dot(point, math.normalize(craterCenter)));
            float xD = distance / craterRadius; // 0 au centre, 1 au bord

            // N'appliquer le cratère que dans la zone concernée
            if (xD < 1f)
            {
                float craterShape = CraterShape(xD);
                vertices[index] = point * (radius + craterShape);
            }
            else
            {
                vertices[index] = point * radius;
            }


        }


    }

    float CraterShape(float x)
    {
        return math.max(math.min(CavityShape(x), RimShape(x)), FloorShape(x));
    }

    float CavityShape(float x)
    {
        return x * x - 1;
    }

    float RimShape(float x)
    {
        float rimWidth = 1.87f;
        float RimSteepness = 0.3f;
        x = math.abs(x) - 1 - rimWidth;
        return RimSteepness * x * x;
    }

    float FloorShape(float x)
    {
        float floorHeight = -1f; // ← négatif pour créer un creux
        return floorHeight;
    }



    float GetPlanetHeight(Vector3 p)
    {
        float continent = continentNoise.GetNoise(p.x, p.y, p.z);
        float seaLevel = -oceanScale;

        if (continent < seaLevel)
            return continent * 0.3f * noiseScale1;
        float landMask = InverseLerp(seaLevel, seaLevel + 0.3f, continent);
        landMask = math.smoothstep(0f, 1f, landMask);

        float warpStrength = 0.4f;

        float wx = warpNoise.GetNoise(p.x + 100f, p.y, p.z) * warpStrength;
        float wy = warpNoise.GetNoise(p.x, p.y + 100f, p.z) * warpStrength;
        float wz = warpNoise.GetNoise(p.x, p.y, p.z + 100f) * warpStrength;

        float mountain = mountainNoise.GetNoise(p.x + wx, p.y + wy, p.z + wz);
        mountain = math.max(0f, mountain);

        float mountainMask = mountainMaskNoise.GetNoise(p.x + 50f, p.y + 50f, p.z + 50f);
        mountainMask = math.smoothstep(0f, 1f, InverseLerp(-0.1f, 0.6f, mountainMask));

        float plains = continent * noiseScale1;
        float mountains = mountain * mountainMask * landMask * noiseScale2 * 3f;

        return plains + mountains;
    }

    float InverseLerp(float a, float b, float value)
    {
        return math.clamp((value - a) / (b - a), 0f, 1f);
    }
}
