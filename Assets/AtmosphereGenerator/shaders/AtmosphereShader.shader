Shader "Custom/AtmosphereShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _planetCentre("Centre de la Planète", Vector) = (0,0,0,0)
        _planetRadius("Rayon de la planète", Float) = 0.5
        _atmosphereRadius("Rayon de l'atmosphère", Float) = 1.0
        _densityFalloff("Chute de densité", Float) = 4.0
        _numOpticalDepthPoints("Échantillons Optical Depth", Float) = 8
        _numInScatteringPoints("Échantillons In-Scattering", Float) = 8
        _dirToSun("Direction du Soleil", Vector) = (0,1,0,0)
    } 

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Math.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D _BakedOpticalDepth;
            sampler2D _BlueNoise;
            float3 _planetCentre;
            float _planetRadius;
            float _atmosphereRadius;
            float _densityFalloff;
            float _numOpticalDepthPoints;
            float _numInScatteringPoints;
            float3 _dirToSun;
            float3 _scaterringCoefficients;
            float _ditherStrength;
            float _ditherScale;
            float intensity;

            struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));
                return o;
            }

            float2 squareUV(float2 uv) {
				float width = _ScreenParams.x;
				float height =_ScreenParams.y;
				//float minDim = min(width, height);
				float scale = 1000;
				float x = uv.x * width;
				float y = uv.y * height;
				return float2 (x/scale, y/scale);
			}

            float densityAtPoint(float3 densitySamplePoint)
            {
                float heightAboveSurface = length(densitySamplePoint - _planetCentre) - _planetRadius;
                float height01 = heightAboveSurface / (_atmosphereRadius - _planetRadius);
                float localDensity = exp(-height01 * _densityFalloff) * (1 - height01);
                return localDensity;
            }

            float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength)
            {
                float3 densitySamplePoint = rayOrigin;
                float stepSize = rayLength / (_numOpticalDepthPoints - 1);
                float opticalDepth = 0;
                for (int i = 0; i < _numOpticalDepthPoints; i++)
                {
                    float localDensity = densityAtPoint(densitySamplePoint);
                    opticalDepth += localDensity * stepSize;
                    densitySamplePoint += rayDir * stepSize;
                }
                return opticalDepth;
            }

            float opticalDepthBaked(float3 rayOrigin, float3 rayDir) {
				float height = length(rayOrigin - _planetCentre) - _planetRadius;
				float height01 = saturate(height / (_atmosphereRadius - _planetRadius));

				float uvX = 1 - (dot(normalize(rayOrigin - _planetCentre), rayDir) * .5 + .5);
				return tex2Dlod(_BakedOpticalDepth, float4(uvX, height01,0,0));
			}

            float opticalDepthBaked2(float3 rayOrigin, float3 rayDir, float rayLength) {
				float3 endPoint = rayOrigin + rayDir * rayLength;
				float d = dot(rayDir, normalize(rayOrigin-_planetCentre));
				float opticalDepth = 0;

				const float blendStrength = 1.5;
				float w = saturate(d * blendStrength + .5);
				
				float d1 = opticalDepthBaked(rayOrigin, rayDir) - opticalDepthBaked(endPoint, rayDir);
				float d2 = opticalDepthBaked(endPoint, -rayDir) - opticalDepthBaked(rayOrigin, -rayDir);

				opticalDepth = lerp(d2, d1, w);
				return opticalDepth;
			}

            float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 originalCol, float2 uv)
            {
                float blueNoise = tex2Dlod(_BlueNoise, float4(squareUV(uv) * _ditherScale,0,0));
				blueNoise = (blueNoise - 0.5) * _ditherStrength;

                float3 inScatterPoint = rayOrigin;
                float stepSize = rayLength / (_numInScatteringPoints - 1);
                float3 inScatteredLight = 0;
                float viewRayOpticalDepth = 0;

                for (int i = 0; i < _numInScatteringPoints; i ++) {
					float sunRayLength = raySphere(_planetCentre, _atmosphereRadius, inScatterPoint, _dirToSun).y;
					float sunRayOpticalDepth = opticalDepthBaked(inScatterPoint +_dirToSun * _ditherStrength, _dirToSun);
					float localDensity = densityAtPoint(inScatterPoint);
					viewRayOpticalDepth = opticalDepthBaked2(rayOrigin, rayDir, stepSize * i);
					float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * _scaterringCoefficients);
					
					inScatteredLight += localDensity * transmittance;
					inScatterPoint += rayDir * stepSize;
				}
                inScatteredLight *= _scaterringCoefficients * intensity * stepSize / _planetRadius;
				inScatteredLight += blueNoise * 0.01;

				// Attenuate brightness of original col (i.e light reflected from planet surfaces)
				// This is a hacky mess, TODO: figure out a proper way to do this
				const float brightnessAdaptionStrength = 0.15;
				const float reflectedLightOutScatterStrength = 3;
				float brightnessAdaption = dot (inScatteredLight,1) * brightnessAdaptionStrength;
				float brightnessSum = viewRayOpticalDepth * intensity * reflectedLightOutScatterStrength + brightnessAdaption;
				float reflectedLightStrength = exp(-brightnessSum);
				float hdrStrength = saturate(dot(originalCol,1)/3-1);
				reflectedLightStrength = lerp(reflectedLightStrength, 1, hdrStrength);
				float3 reflectedLight = originalCol * reflectedLightStrength;

				float3 finalCol = reflectedLight + inScatteredLight;

				
				return finalCol;
            }

            float4 frag(v2f i) : SV_Target
            {
                //_dirToSun = normalize(-_WorldSpaceLightPos0.xyz);
                float4 originalCol = tex2D(_MainTex, i.uv);
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float2 hit = raySphere(_planetCentre, _atmosphereRadius, rayOrigin, rayDir);
                float dstTo = hit.x;
                float dstThrough = min(hit.y, sceneDepth - dstTo); 

                if (dstThrough > 0)
                {
                    const float epsilon = 0.0001;
                    float3 pointInAtmosphere = rayOrigin + rayDir * (dstTo + epsilon);
                    float3 light = calculateLight(pointInAtmosphere, rayDir, dstThrough - epsilon*2, originalCol, i.uv);
                    return float4(light, 1);
                }

                return originalCol;
            }

            ENDCG
        }
    }
}
