Shader "StubbedRoR2/Base/Shaders/FX/HGForwardPlanet" {
	Properties {
		[Header(Blending)] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendFloat ("Source Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlendFloat ("Destination Blend", Float) = 1
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Float) = 0
		[Header(Lighting and Base Colors)] [HDR] _TintColor ("Tint", Vector) = (1,1,1,1)
		[NoScaleOffset] _LightWarpRamp ("Lightwarp Ramp", 2D) = "grey" {}
		_DetailStrength ("Detail Strength", Range(0, 1)) = 0.5
		_DiffuseTex ("Diffuse Texture", 2D) = "white" {}
		_DiffuseDetailTex ("Diffuse Detail Texture", 2D) = "white" {}
		_NormalStrength ("Normal Strength", Range(0, 5)) = 1
		_NormalTex ("Normal Map", 2D) = "bump" {}
		_NormalDetailTex ("Normal Detail Map", 2D) = "bump" {}
		_SpecColor ("Specular Color", Vector) = (1,1,1,1)
		_SpecularPower ("Specular Power", Range(0.1, 1000)) = 1
		_DoubleSampleFactor ("Double Sample Factor", Range(0, 5)) = 1
		[Header(Atmosphere)] [NoScaleOffset] _AtmosphereRamp ("Atmosphere Ramp", 2D) = "grey" {}
		_AtmosphereStrength ("Atmosphere Strength", Range(0, 20)) = 1
		[Header(Emission)] _EmissionTex ("Emission Texture", 2D) = "black" {}
		_EmissionRamp ("Emission Ramp", 2D) = "white" {}
		_EmissionStrength ("Emission Strength", Range(0, 100)) = 1
		[Header(Animation)] _RotationSpeed ("Rotation Speed", Range(0, 10)) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}