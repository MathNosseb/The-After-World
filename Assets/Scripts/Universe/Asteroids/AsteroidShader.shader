Shader "Custom/AsteroidShader"
{
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            Cull Off
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"
            #include "Lighting.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float uid : TEXCOORD1;
            };

            float4x4 _ObjectToWorld;
            float4 _Color;
            StructuredBuffer<float3> _AsteroidsPosition;

            v2f vert(float4 vertex : POSITION, float3 normal : NORMAL, uint instanceID : SV_InstanceID)
            {
                v2f o;

                float3 instancePos = _AsteroidsPosition[instanceID];

                float3 worldPos = mul(_ObjectToWorld, float4(instancePos + (vertex.xyz*500), 1.0)).xyz;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.uid = instanceID;
                o.normal = normal;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return _Color;
            }

            ENDCG
        }
    }

}
