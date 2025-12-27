Shader "Custom/PlanetHeightTextures"
{
    Properties
    {
        _LowTex ("Low Texture", 2D) = "white" {}
        _MidTex ("Mid Texture", 2D) = "white" {}
        _HighTex ("High Texture", 2D) = "white" {}
        _MinRadius ("Min Radius", Float) = 0.95
        _MaxRadius ("Max Radius", Float) = 1.05
        _BlendSharpness ("Blend Sharpness", Float) = 4.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float height : TEXCOORD1;
            };

            sampler2D _LowTex;
            sampler2D _MidTex;
            sampler2D _HighTex;
            float _MinRadius;
            float _MaxRadius;
            float _BlendSharpness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Distance du vertex au centre local de la planète
                float radius = length(v.vertex.xyz);
                o.height = saturate((radius - _MinRadius) / (_MaxRadius - _MinRadius));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Hauteur normalisée
                float h = i.height;

                // Lissage du mélange
                float lowBlend = saturate(1.0 - h * _BlendSharpness);
                float highBlend = saturate((h - 0.5) * _BlendSharpness);

                // Échantillonnage des textures
                fixed4 low = tex2D(_LowTex, i.uv);
                fixed4 mid = tex2D(_MidTex, i.uv);
                fixed4 high = tex2D(_HighTex, i.uv);

                // Mélange progressif : bas → milieu → haut
                fixed4 color;
                if (h < 0.5)
                    color = lerp(low, mid, smoothstep(0.0, 0.5, h));
                else
                    color = lerp(mid, high, smoothstep(0.5, 1.0, h));

                return color;
            }
            ENDCG
        }
    }
}
