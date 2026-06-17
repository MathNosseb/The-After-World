Shader "Custom/GrassShader"
{
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            Cull Back
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"
            #include "Lighting.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float uid : TEXCOORD1;
                float noise : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float3 worldNormal : TEXCOORD4;
            };

            struct BladeData {
                float3 position;//object space
                float4 rotation;
                float noise;
            };

            float4x4 _ObjectToWorld;
            float4 _ColorBase;
            float4 _ColorTip;
            StructuredBuffer<BladeData> _Blades;
            float3 _dirToSun;
            float3 playerPosition;

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

            v2f vert(float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0, uint instanceID : SV_InstanceID, float3 worldNormal : TEXCOORD4)
            {
                UNITY_SETUP_INSTANCE_ID(instanceID);

                v2f o;

                o.uv = uv;

                //pos en ObjectSpace
                float3 pos = _Blades[instanceID].position;

                float4 rot = normalize(_Blades[instanceID].rotation);

                float4 qConjugue = float4(-rot.x, -rot.y, -rot.z, rot.w);
                    
                float3 worldPos = rotateVector(vertex.xyz*0.5, normalize(_Blades[instanceID].rotation)) + mul(_ObjectToWorld, float4(pos, 1.0)).xyz;

                //pos en worldSpace
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.uid = instanceID;
                o.noise = _Blades[instanceID].noise;
                
                o.worldNormal = rotateVector(normal, rot);
                

                return o;
            }

            


            float4 frag(v2f i) : SV_Target
            {
                float variation = lerp(0.7, 1.3, i.noise);
                float3 color = lerp(_ColorBase.rgb, _ColorTip.rgb, i.uv.y) * variation;

                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                float NdotL = saturate(dot(normal, lightDir));

                float3 ambient = color.rgb * float3(0,0.3,1);
                float3 diffuse = color.rgb * NdotL;
                return float4(ambient + diffuse, 1.0);


                // return float4(diffuse, 1.0);
            }
            ENDCG
        }
    }
}
