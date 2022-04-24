Shader "StubbedRoR2/Base/Shaders/FX/HGSolidParallax" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissionTex ("Emission (RGB)", 2D) = "black" {}
		_EmissionPower ("Emission Power", Range(0.1, 20)) = 1
		_Normal ("Normal", 2D) = "bump" {}
		_SpecularStrength ("Specular Strength", Range(0, 1)) = 0
		_SpecularExponent ("Specular Exponent", Range(0.1, 20)) = 0.1
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Height1 ("Height 1", 2D) = "white" {}
		_Height2 ("Height 2", 2D) = "white" {}
		_HeightStrength ("Height Strength", Range(0, 20)) = 1
		_HeightBias ("Height Bias", Range(0, 1)) = 0
		_Parallax ("Parallax", Float) = 0
		_ScrollSpeed ("Height Scroll Speed", Vector) = (0,0,0,0)
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Float) = 2
		[Toggle(ALPHACLIP)] _ClipOn ("Alpha Clip", Float) = 0
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