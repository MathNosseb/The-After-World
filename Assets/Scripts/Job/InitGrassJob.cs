using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.VisualScripting;


[BurstCompile]
public struct InitGrassJob : IJobParallelFor
{
    public int density;
    public float spread;
    
    public float minDistance;
    public float maxDistance;
    public float maxAngleTerrain;
    public float planetRadius;
    public float seed;
    public float3 planetPosition;
    public float3 PlanetFacedirection;


    [ReadOnly] public NativeArray<Vector3> verts;
    [ReadOnly] public NativeArray<Vector2> uvs;
    [ReadOnly] public NativeArray<Vector3> normals;
    public NativeArray<Vector3> positions;
    public NativeArray<quaternion> rotations;
    public NativeArray<float> noises;
    public NativeArray<bool> valid;


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
        int i = index / density;

        float3 worldPos = verts[i];//espace local (planete)

        float3 N = normals[i];//la normal -> vers l exterieur de la planete
        Vector3 arbitrary = (Mathf.Abs(N.y) < 0.99f) ? Vector3.up : Vector3.right;//choix d un vecteur arbitraire
        float3 T = math.normalize(math.cross(N, arbitrary));//tangente a la surface
        float3 B = math.cross(N, T);//deuxieme direction tangentielle
        //generation de nombre pseudo-aleatoire
        Unity.Mathematics.Random rand = new Unity.Mathematics.Random(
                                            (uint)math.hash(
                                                new uint2(
                                                    (uint)index, 
                                                    (uint)seed)
                                            )
                                        );

        ///Generation d'une position aleatoire sur la position initiale par
        ///rapport au vertice
        /// 
        //generation d'un angle aleatoire pour la rotation sur Axe Y
        float angle = rand.NextFloat(0f, math.PI * 2f);
        //direction alatoire dans une sphere imaginaire
        float radiusRand = math.sqrt(rand.NextFloat());
        //point dans tout le disque (centre + bords)
        //point 2d dans un disque uniforme
        float2 p = new float2(
            math.cos(angle) * radiusRand,
            math.sin(angle) * radiusRand
        );
        //projection en 3D + spread
        float2 randPos = p * spread;
        float3 offset = T * randPos.x + B * randPos.y;
        float3 localPositionOffset = offset + worldPos;

        float3 samplePos = worldPos + offset;
        float3 direction = math.normalize(samplePos - planetPosition); //direction

        //construction d'un cube
        Vector3 localUp = PlanetFacedirection;
        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);
        Vector2 percent = uvs[i];
        //projection en cube sphere
        Vector3 point = localUp +
                        (percent.x - 0.5f) * 2f * axisA +
                        (percent.y - 0.5f) * 2f * axisB;
        point = point.normalized;
        float h = GetPlanetHeight(point);
        float3 position = point * (planetRadius + h);

        float distance = math.length(position);

        float3 planetUp = math.normalize(position);
        float angleFromNormal = math.dot(N, planetUp);

        //check de la hauteur du terrain (max distance et min distance)
        //check de l angle max du terrain pour ne pas spawn sur des montagnes
        if (distance <= maxDistance && distance >= minDistance && angleFromNormal >= maxAngleTerrain)
        {
            positions[index] = position;
        
            noises[index] = noise.pnoise(localPositionOffset.xy * 0.1f, new float2(100f,100f));

            // rotation qui aligne Y → up
            quaternion align = FromToRotation(new float3(0,1,0), planetUp);

            // rotation aléatoire autour de la normale
            float randomAngle = rand.NextFloat(0f, math.PI * 2f);
            quaternion randomRot = quaternion.AxisAngle(planetUp, randomAngle);

            // combinaison
            Quaternion finalRot = math.mul(randomRot, align);

            rotations[index] = finalRot;
            valid[index] = true;
        }
        else
            valid[index] = false;

        
    }

    /// <summary>
    /// transforme une position 3D sur la sphere en hauteur terrain procedural
    /// chaine de bruits empilé
    /// </summary>
    /// <param name="p">direction de la sphere au vertices</param>
    /// <returns>hauteur local au point p</returns>
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

    public quaternion FromToRotation(float3 from, float3 to)
    {
        float3 axis = math.cross(from, to);
        float sinAngle = math.length(axis);
        float cosAngle = math.dot(from, to);

        // Cas quasi-parallèle → identité
        if (sinAngle < 1e-6f)
            return cosAngle > 0f ? quaternion.identity
                                : quaternion.AxisAngle(new float3(1,0,0), math.PI);

        return quaternion.AxisAngle(axis / sinAngle, math.atan2(sinAngle, cosAngle));
    }
}
