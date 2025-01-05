Shader "Moonstorm/FX/MSColorRampScroll"
{
    Properties
    {
        _Tint("Tint: ", Color) = (1,1,1,1)
        _Ramp("Ramp: ", 2D) = "white"{}
        _ScrollVector("Scroll Vector: ", Vector) = (1, 0, 0, 0)
        [MaterialToggle] _Normal("Normal :", Float) = 0
        _NormalTex("Normal Texture: ", 2D) = "black"{}
        _NormalPower("Normal Power: ", Range(0,1)) = 1

        _Gloss ("Gloss: ", Range(0,1)) = 1
        _SpecularExponent ("Specular Exponent: ", Float) = 6
        _SpecularOuterBandThreshold ("Specular Outer Band Threshold: ", Range(0,1)) = 0.7
        _SpecularPower ("Specular Power: ", Range(0,3)) = 0.5

        _DiffuseThreshold ("Diffuse Threshold: ", Range(0,1)) = 0.9
        _DiffusePower ("Diffuse Power: ", Range(0,1)) = 0.2

        [MaterialToggle]_Fresnel ("Fresnel: ", Float) = 0
        _FresnelTint("Fresnel Tint: ", Color) = (1,1,1,1)
        _FresnelRamp("Fresnel Ramp: ", 2D) = "white"{}
        [MaterialToggle]_FresnelBlending ("Fresnel Blending: ", Float) = 0
        _FresnelThreshold ("Fresnel Threshold: ", Range(0,1)) = 0.5
        _FresnelPower ("Fresnel Power: ", Range(0,2)) = 0.5

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            #define SPECULAR_OUTER_BAND_CONST 0.667

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 bitangent : TEXCOORD3;
                float4 local_space : TEXCOORD4;
                float3 wPos : TEXCOORD5;
            };

            fixed4 _Tint;
            sampler2D _Ramp;
            uniform float4 _Ramp_TexelSize;
            uniform float4 _Ramp_ST;
            float4 _ScrollVector;
            float _Normal;
            sampler2D _NormalTex;
            uniform float4 _NormalTex_ST;
            uniform float4 _NormalTex_TexelSize;
            float _NormalPower;

            float _Gloss;
            float _SpecularExponent;
            float _SpecularOuterBandThreshold;
            float _SpecularPower;

            float _DiffuseThreshold;
            float _DiffusePower;

            fixed4 _FresnelTint;
            sampler2D _FresnelRamp;
            uniform float4 _FresnelRamp_TexelSize;
            uniform float4 _FresnelRamp_ST;
            float _Fresnel;
            float _FresnelBlending;
            float _FresnelThreshold;
            float _FresnelPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.bitangent = cross(o.normal, o.tangent) * (v.tangent.w * unity_WorldTransformParams.w);
                o.local_space = v.vertex;
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                _ScrollVector.xyz = normalize(_ScrollVector.xyz);
                float samplePosition = (-dot(i.local_space.xyz, _ScrollVector.xyz) + _Ramp_ST.z) * _Ramp_ST.x;
                float4 sampleColor = tex2D(_Ramp, float2(samplePosition + _Time.y * _ScrollVector.w, 0));
                float3 N;
                if(_Normal){
                    float3 tangentSpaceNormal = UnpackNormal(tex2D(_NormalTex, i.uv.xy * _NormalTex_ST.xy + _NormalTex_ST.zw));
                    tangentSpaceNormal = normalize(lerp(float3(0,0,1), tangentSpaceNormal, _NormalPower));
                    float3x3 mtxTangToWorld = {
                        i.tangent.x, i.bitangent.x, i.normal.x,
                        i.tangent.y, i.bitangent.y, i.normal.y,
                        i.tangent.z, i.bitangent.z, i.normal.z
                    };
                    N = mul(mtxTangToWorld, _Normal ? tangentSpaceNormal : float3(1,1,1));
                }
                else{
                    N = normalize(i.normal.xyz);
                }
                float3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
                float3 lambertian = saturate(dot(N, L));
                float3 diffuse = step(_DiffuseThreshold, lambertian ) * _DiffusePower * _LightColor0.xyz;

                //Speculars
                float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
                float3 H = normalize (L + V);
                float specularExponent = exp2(_Gloss * _SpecularExponent) + 1;
                float3 specularLight = saturate(dot(H, N)) * (lambertian > 0);
                specularLight = pow(specularLight, specularExponent) * _Gloss;
                specularLight = step(0.7, specularLight) * _SpecularPower + step(0.7 * _SpecularOuterBandThreshold , specularLight) * _SpecularPower * _LightColor0.xyz * SPECULAR_OUTER_BAND_CONST;
                
                float fresnelSamplePosition = (-dot(i.local_space.xyz, _ScrollVector.xyz) + _FresnelRamp_ST.z) * _FresnelRamp_ST.x;
                float fresnelFactor = 1 - step(_FresnelThreshold, dot(V, N));
                float3 fresnel = fresnelFactor *  _Fresnel * _FresnelPower * tex2D(_FresnelRamp, float2(fresnelSamplePosition + _Time.y * _ScrollVector.w, 0)) * _FresnelTint;
                
                return _FresnelBlending || !_Fresnel ?
                    float4((sampleColor + diffuse + specularLight + fresnel) * _Tint, 1) :
                    float4(lerp(sampleColor + diffuse * _Tint + specularLight , fresnel, fresnelFactor), 1);
            }

            ENDCG
        }

        Pass{
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            #define SPECULAR_OUTER_BAND_CONST 0.667

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 bitangent : TEXCOORD3;
                float3 wPos : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };

            fixed4 _Tint;
            float _Normal;
            sampler2D _NormalTex;
            uniform float4 _NormalTex_ST;
            uniform float4 _NormalTex_TexelSize;
            float _NormalPower;

            float _Gloss;
            float _SpecularExponent;
            float _SpecularOuterBandThreshold;
            float _SpecularPower;

            float _DiffuseThreshold;
            float _DiffusePower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.bitangent = cross(o.normal, o.tangent) * (v.tangent.w * unity_WorldTransformParams.w);
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 N;
                if(_Normal){
                    float3 tangentSpaceNormal = UnpackNormal(tex2D(_NormalTex, i.uv.xy * _NormalTex_ST.xy + _NormalTex_ST.zw));
                    tangentSpaceNormal = normalize(lerp(float3(0,0,1), tangentSpaceNormal, _NormalPower));
                    float3x3 mtxTangToWorld = {
                        i.tangent.x, i.bitangent.x, i.normal.x,
                        i.tangent.y, i.bitangent.y, i.normal.y,
                        i.tangent.z, i.bitangent.z, i.normal.z
                    };
                    N = mul(mtxTangToWorld, _Normal ? tangentSpaceNormal : float3(1,1,1));
                }
                else{
                    N = normalize(i.normal);
                }
                float3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
                float attenuation = LIGHT_ATTENUATION(i);
                float3 lambertian = saturate(dot(N, L));
                float3 diffuse = step(_DiffuseThreshold , lambertian * attenuation )  * _DiffusePower * _LightColor0.xyz;

                //Speculars
                float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
                float3 H = normalize (L + V);
                float specularExponent = exp2(_Gloss * _SpecularExponent) + 1;
                float3 specularLight = saturate(dot(H, N)) * (lambertian > 0);
                specularLight = pow(specularLight, specularExponent) * _Gloss * attenuation;
                specularLight = step(0.7, specularLight) * _SpecularPower + step(0.7 * _SpecularOuterBandThreshold , specularLight) * _SpecularPower * _LightColor0.xyz * SPECULAR_OUTER_BAND_CONST;
                
                return float4(diffuse * _Tint + specularLight  , 1);
            }

            ENDCG
        }
        
    }
}
