Shader "StubbedRoR2/Base/Shaders/FX/HGCloudRemap" {
	Properties {
		[HideInInspector] _SrcBlend ("Source Blend", Float) = 1
		[HideInInspector] _DstBlend ("Destination Blend", Float) = 1
		[HideInInspector] _InternalSimpleBlendMode ("Internal Simple Blend Mode", Float) = 0
		[HDR] _TintColor ("Tint", Vector) = (1,1,1,1)
		[Toggle(DISABLEREMAP)] _DisableRemapOn ("Disable Remapping", Float) = 0
		_MainTex ("Base (RGB) Trans (A)", 2D) = "grey" {}
		_RemapTex ("Color Remap Ramp (RGB)", 2D) = "grey" {}
		_InvFade ("Soft Factor", Range(0, 2)) = 0.1
		_Boost ("Brightness Boost", Range(1, 20)) = 1
		_AlphaBoost ("Alpha Boost", Range(0, 20)) = 1
		_AlphaBias ("Alpha Bias", Range(0, 1)) = 0
		[Toggle(USE_UV1)] _UseUV1On ("Use UV1", Float) = 0
		[Toggle(FADECLOSE)] _FadeCloseOn ("Fade when near camera", Float) = 0
		_FadeCloseDistance ("Fade Close Distance", Range(0, 1)) = 0.5
		[MaterialEnum(None,0,Front,1,Back,2)] _Cull ("Culling Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
		_DepthOffset ("_DepthOffset", Range(-10, 10)) = 0
		[Toggle(USE_CLOUDS)] _CloudsOn ("Cloud Remapping", Float) = 1
		[Toggle(CLOUDOFFSET)] _CloudOffsetOn ("Distortion Clouds", Float) = 0
		_DistortionStrength ("Distortion Strength", Range(-2, 2)) = 0.1
		_Cloud1Tex ("Cloud 1 (RGB) Trans (A)", 2D) = "grey" {}
		_Cloud2Tex ("Cloud 2 (RGB) Trans (A)", 2D) = "grey" {}
		_CutoffScroll ("Cutoff Scroll Speed", Vector) = (0,0,0,0)
		[Toggle(VERTEXCOLOR)] _VertexColorOn ("Vertex Colors", Float) = 0
		[Toggle(VERTEXALPHA)] _VertexAlphaOn ("Luminance for Vertex Alpha", Float) = 0
		[Toggle(CALCTEXTUREALPHA)] _CalcTextureAlphaOn ("Luminance for Texture Alpha", Float) = 0
		[Toggle(VERTEXOFFSET)] _VertexOffsetOn ("Vertex Offset", Float) = 0
		[Toggle(FRESNEL)] _FresnelOn ("Fresnel Fade", Float) = 0
		[Toggle(SKYBOX_ONLY)] _SkyboxOnly ("Skybox Only", Float) = 0
		_FresnelPower ("Fresnel Power", Range(-20, 20)) = 0
		_OffsetAmount ("Vertex Offset Amount", Range(0, 3)) = 0
		[PerRendererData] _ExternalAlpha ("External Alpha", Range(0, 1)) = 1
		[PerRendererData] _Fade ("Fade", Range(0, 1)) = 1
	}
	//DummyShaderTextExporter
	Category 
	{
		SubShader
		{
		LOD 0
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			Pass {
				CGPROGRAM
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#include "UnityCG.cginc"
				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};
				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					
				};
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif
				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz += _SinTime.xyz;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					

					fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord.xy*_MainTex_ST.xy + _MainTex_ST.zw );
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}

	Fallback "Transparent/VertexLit"
	//CustomEditor "RoR2.HopooCloudRemapGUI"
}