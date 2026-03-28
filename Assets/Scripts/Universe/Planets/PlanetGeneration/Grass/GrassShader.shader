Shader "Custom/GrassShader"
{
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Pass
        {
            Cull Off
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float uid : TEXCOORD1;
                float noise : TEXCOORD2;
            };

            float4x4 _ObjectToWorld;
            StructuredBuffer<float3> _Positions;
            float4 _Color;
            StructuredBuffer<float4> _Rotations;
            StructuredBuffer<float> _Noises;
            float3 _dirToSun;

            float4 quatMul(float4 a, float4 b)
            {
                return float4(
                    a.w*b.x + a.x*b.w + a.y*b.z - a.z*b.y,
                    a.w*b.y - a.x*b.z + a.y*b.w + a.z*b.x,
                    a.w*b.z + a.x*b.y - a.y*b.x + a.z*b.w,
                    a.w*b.w - a.x*b.x - a.y*b.y - a.z*b.z
                );
            }

            float4 quatConjugate(float4 q)
            {
                return float4(-q.x, -q.y, -q.z, q.w);
            }

            float3 rotateVector(float3 v, float4 q)
            {
                float3 t = 2 * cross(q.xyz, v);
                return v + q.w * t + cross(q.xyz, t);
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert(float4 vertex : POSITION, float3 normal : NORMAL, uint instanceID : SV_InstanceID)
            {
                UNITY_SETUP_INSTANCE_ID(vertex);

                v2f o;

                float3 pos = _Positions[instanceID];

                float4 rot = normalize(_Rotations[instanceID]);

                float4 qConjugue = float4(-rot.x, -rot.y, -rot.z, rot.w);

                /*
                float angle = _Rotations[instanceID];
                float rad = radians(angle);

                float s = sin(rad);
                float c = cos(rad);

                float3 v = vertex.xyz;

                // rotation autour de Y
                float3 rotated;
                rotated.x = v.x * c - v.z * s;
                rotated.z = v.x * s + v.z * c;
                rotated.y = v.y;
                */
                // position finale
                //float3 worldPos = rotated + _Positions[instanceID];
                    
                float3 worldPos = rotateVector(vertex.xyz, normalize(_Rotations[instanceID])) + mul(_ObjectToWorld, float4(pos, 1.0)).xyz;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.uid = instanceID;
                o.noise = _Noises[instanceID];
                // Normale fixe (important pour éviter les artefacts)
                o.normal = float3(0,1,0);

                return o;
            }

            


            float4 frag(v2f i) : SV_Target
            {
                //float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = abs(dot(normalize(i.normal), _dirToSun));
                float r = frac(sin(i.uid * 12.9898) * 43758.5453);
                //float3 baseColor = float3(0.2, 0.8, 0.2);
                float variation = lerp(0.7, 1.3, (r*i.noise));

                float3 color = _Color * variation;
                //return float4(0.2, 0.8, 0.2, 1.0);
                //return float4(0.2, 0.8, 0.2, 1.0);
                return _Color * variation;
            }
            ENDCG
        }
    }
}
