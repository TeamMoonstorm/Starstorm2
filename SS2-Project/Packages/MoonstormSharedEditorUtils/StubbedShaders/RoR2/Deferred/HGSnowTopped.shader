Shader "StubbedRoR2/Base/Shaders/Deferred/HGSnowTopped" {
	Properties {
		[Header(Main Properties)] _Color ("Main Color", Vector) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NormalStrength ("Normal Strength", Range(0, 1)) = 1
		_NormalTex ("Normal Map", 2D) = "bump" {}
		_SpecularStrength ("Base Specular Strength", Range(0, 1)) = 0
		_SpecularExponent ("Base Specular Exponent", Range(0, 20)) = 1
		_Smoothness ("Base Smoothness", Range(0, 1)) = 0
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		[PerRendererData] _Fade ("Fade", Range(0, 1)) = 1
		[Header(Snow Properties)] _SnowTex ("Snow Texture (RGB) Depth (A)", 2D) = "white" {}
		[NoScaleOffset] _SnowNormalTex ("Snow Normal Map", 2D) = "bump" {}
		_SnowSpecularStrength ("Snow Specular Strength", Range(0, 1)) = 0
		_SnowSpecularExponent ("Snow Specular Exponent", Range(0, 20)) = 1
		_SnowSmoothness ("Snow Smoothness", Range(0, 1)) = 0
		_SnowBias ("Snow Y Bias", Range(-1, 1)) = 0
		_Depth ("Blend Depth", Range(0, 1)) = 0.2
		[Header(Triplanar Properties)] [Toggle(TRIPLANAR)] _TriplanarOn ("Enable Snow Triplanar Projection", Float) = 0
		_TriplanarTextureFactor ("Triplanar Projection Texture Factor", Range(0, 1)) = 1
		[Header(WorldSpace Gradient Bias)] [Toggle(GRADIENTBIAS)] _GradientBiasOn ("Enable World-Size Gradient Bias", Float) = 0
		_GradientBiasVector ("World-Size Gradient Bias Vector", Vector) = (0,0,0,0)
		[Header(Blue Channel Dirt)] [Toggle(DIRTON)] _DirtOn ("Enable Blue-Channel Dirt", Float) = 0
		_DirtTex ("Dirt Texture (RGB) Depth (A)", 2D) = "white" {}
		_DirtNormalTex ("Dirt Normal Map", 2D) = "bump" {}
		_DirtBias ("Dirt Bias", Range(-2, 2)) = 0
		_DirtSpecularStrength ("Dirt Specular Strength", Range(0, 1)) = 0
		_DirtSpecularExponent ("Dirt Specular Exponent", Range(0, 20)) = 1
		_DirtSmoothness ("Dirt Smoothness", Range(0, 1)) = 0
		[Header(Misc Shader Features)] [Toggle(DITHER)] _DitherOn ("Enable Dither", Float) = 0
		[Toggle(IGNORE_BIAS)] _IgnoreBiasOn ("Ignore Alpha Weights", Float) = 0
		[Toggle(BINARYBLEND)] _BinaryBlendOn ("Blend Weights Binarily", Float) = 1
		[Toggle(MICROFACET_SNOW)] _SnowOn ("Enable Snow Microfacets", Float) = 0
		[Toggle(FORCE_SPEC)] _ForceSpecOn ("Ignore Diffuse Alpha for Speculars", Float) = 0
		[Toggle(VERTEXCOLOR)] _VertexColorOn ("Use Vertex Colors for Weights", Float) = 0
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