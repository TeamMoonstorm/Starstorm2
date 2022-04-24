Shader "StubbedRoR2/Base/Shaders/Environment/HGWaterfall" {
	Properties {
		_Color ("Main Color", Vector) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NormalTex ("Normal Map", 2D) = "bump" {}
		_NormalStrength ("Normal Strength", Range(0, 1)) = 1
		_VertexOffsetStrength ("Vertex Offset Strength", Range(0, 5)) = 1
		_PerlinScale ("Perlin Scale", Vector) = (0,0,0,0)
		_Cutoff ("Alpha Cutoff", Range(0, 2)) = 0.5
		_FoamTex ("Foam Texture", 2D) = "white" {}
		_Depth ("Blend Depth", Range(0, 1)) = 0.2
		_Speed ("Water Speed", Range(-5, 5)) = 0.1
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		_SpecularStrength ("Specular Strength", Range(0, 1)) = 0
		_SpecularExponent ("Specular Exponent", Range(0, 20)) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}