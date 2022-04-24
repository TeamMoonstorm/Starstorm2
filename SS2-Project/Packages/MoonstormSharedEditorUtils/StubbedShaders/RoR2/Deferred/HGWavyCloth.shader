Shader "StubbedRoR2/Base/Shaders/Deferred/HGWavyCloth" {
	Properties {
		_Color ("Main Color", Vector) = (0.5,0.5,0.5,1)
		_Cutoff ("Alpha Cutoff Value", Range(0, 1)) = 0.5
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ScrollingNormalMap ("Scrolling Normal Map", 2D) = "bump" {}
		_NormalStrength ("Normal Strength", Range(0, 5)) = 0
		_Scroll ("Scroll Speed (XY), Distortion Noise Scale (ZW)", Vector) = (0,0,0,0)
		_VertexOffsetStrength ("Vertex Offset Strength", Range(0, 5)) = 0
		_WindVector ("Wind Offset Vector", Vector) = (0,0,0,0)
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		_SpecularStrength ("Specular Strength", Range(0, 1)) = 0
		_SpecularExponent ("Specular Exponent", Range(0, 20)) = 1
		[Toggle(VERTEX_RED_FOR_DISTORTION)] _EnableVertexColorDistortion ("Use Vertex R channel for distortion strength", Float) = 0
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