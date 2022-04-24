Shader "StubbedCalm Water/Shaders/CalmWater - DX11" {
	Properties {
		_Color ("Shallow Color", Vector) = (1,1,1,1)
		_DepthColor ("Depth Color", Vector) = (0,0,0,0)
		_Depth ("Depth", Float) = 0.5
		[Toggle(_DEPTHFOG_ON)] _EnableFog ("Enable Depth Fog", Float) = 0
		_EdgeFade ("Edge Fade", Float) = 1
		_SpecColor ("SpecularColor", Vector) = (1,1,1,1)
		_Smoothness ("Smoothness", Range(0.01, 5)) = 0.5
		_BumpMap ("Micro Detail", 2D) = "bump" {}
		_BumpStrength ("Bump Strength", Range(0, 1)) = 1
		[Toggle(_BUMPLARGE_ON)] _EnableLargeBump ("Enable Large Detail", Float) = 0
		_BumpMapLarge ("Large Detail", 2D) = "bump" {}
		_BumpLargeStrength ("Bump Large Strength", Range(0, 1)) = 1
		[Toggle(_WORLDSPACE_ON)] _WorldSpace ("World UV", Float) = 0
		_Speeds ("Speeds", Vector) = (0.5,0.5,0.5,0.5)
		_SpeedsLarge ("Speeds Large", Vector) = (0.5,0.5,0,0)
		_Distortion ("Distortion", Range(0, 100)) = 50
		[KeywordEnum(High,Low)] _DistortionQuality ("Distortion Quality", Float) = 0
		[KeywordEnum(None,Mixed,RealTime,CubeMap)] _ReflectionType ("ReflectionType", Float) = 0
		_CubeColor ("CubeMap Color [RGB] Intensity [A]", Vector) = (1,1,1,1)
		[NoScaleOffset] _Cube ("CubeMap", Cube) = "black" {}
		[NoScaleOffset] _ReflectionTex ("Internal reflection", 2D) = "white" {}
		_Reflection ("Reflection", Range(0, 1)) = 1
		_RimPower ("Fresnel Angle", Range(1, 20)) = 5
		[Toggle(_FOAM_ON)] _FOAM ("Enable Foam", Float) = 0
		_FoamColor ("FoamColor", Vector) = (1,1,1,1)
		_FoamTex ("Foam Texture", 2D) = "white" {}
		_FoamSize ("Fade Size", Float) = 0.5
		[KeywordEnum(Off,Wave,Gerstner)] _DisplacementMode ("Mode", Float) = 0
		_Amplitude ("Amplitude", Float) = 0.05
		_Frequency ("Frequency", Float) = 1
		_Speed ("Wave Speed", Float) = 1
		_Steepness ("Wave Steepness", Float) = 1
		_WSpeed ("Wave Speed", Vector) = (1.2,1.375,1.1,1.5)
		_WDirectionAB ("Wave1 Direction", Vector) = (0.3,0.85,0.85,0.25)
		_WDirectionCD ("Wave2 Direction", Vector) = (0.1,0.9,0.5,0.5)
		_Smoothing ("Smoothing", Range(0, 1)) = 1
		_Tess ("Tessellation", Range(1, 32)) = 4
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "CalmWater/Calm Water [Single Sided]"
	//CustomEditor "CalmWaterInspector"
}