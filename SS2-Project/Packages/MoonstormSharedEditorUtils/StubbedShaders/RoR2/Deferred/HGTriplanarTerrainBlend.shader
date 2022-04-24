Shader "StubbedRoR2/Base/Shaders/Deferred/HGTriplanarTerrainBlend" {
	Properties {
		[Toggle(USE_VERTEX_COLORS)] _ColorsOn ("Use Vertex Colors Instead", Float) = 0
		[Toggle(MIX_VERTEX_COLORS)] _MixColorsOn ("Mix Vertex Colors with Texture", Float) = 0
		[Toggle(USE_ALPHA_AS_MASK)] _MaskOn ("Use Alpha Channels as Weight Mask", Float) = 0
		[Toggle(USE_VERTICAL_BIAS)] _VerticalBiasOn ("Bias Green Channel to Vertical", Float) = 0
		[Toggle(DOUBLESAMPLE)] _DoublesampleOn ("Double Sample UVs", Float) = 1
		_Color ("Main Color", Vector) = (0.5,0.5,0.5,1)
		_NormalTex ("Normal Tex (RGB)", 2D) = "bump" {}
		_NormalStrength ("Normal Strength", Range(0, 1)) = 1
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		[MaterialEnum(Default,0,Environment,1,Character,2, Misc,3)] _DecalLayer ("Decal Layer", Float) = 0
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Float) = 2
		_TextureFactor ("Texture Factor", Range(0, 1)) = 1
		_Depth ("Blend Depth", Range(0, 1)) = 0.2
		_SplatmapTex ("Splatmap Tex (RGB)", 2D) = "white" {}
		_RedChannelTopTex ("Red Channel Top Albedo (RGB) Specular Scale (A)", 2D) = "white" {}
		_RedChannelSideTex ("Red Channel Side Albedo (RGB) Specular Scale (A)", 2D) = "white" {}
		_RedChannelSmoothness ("Red Channel Smoothness", Range(0, 1)) = 0
		_RedChannelSpecularStrength ("Red Channel Specular Strength", Range(0, 1)) = 0
		_RedChannelSpecularExponent ("Red Channel Specular Exponent", Range(0.1, 20)) = 0
		_RedChannelBias ("Red Channel Bias", Range(-2, 5)) = 0
		_GreenChannelTex ("Green Channel Albedo (RGB) Specular Scale (A)", 2D) = "white" {}
		_GreenChannelSmoothness ("Green Channel Smoothness", Range(0, 1)) = 0
		_GreenChannelSpecularStrength ("Green Channel Specular Strength", Range(0, 1)) = 0
		_GreenChannelSpecularExponent ("Green Channel Specular Exponent", Range(0.1, 20)) = 0
		_GreenChannelBias ("Green Channel Bias", Range(-2, 5)) = 0
		_BlueChannelTex ("Blue Channel Albedo  Specular Scale (A)", 2D) = "white" {}
		_BlueChannelSmoothness ("Blue Channel Smoothness", Range(0, 1)) = 0
		_BlueChannelSpecularStrength ("Blue Channel Specular Strength", Range(0, 1)) = 0
		_BlueChannelSpecularExponent ("Blue Channel Specular Exponent", Range(0.1, 20)) = 0
		_BlueChannelBias ("Blue Channel Bias", Range(-2, 5)) = 0
		[Toggle(MICROFACET_SNOW)] _SnowOn ("Treat G.Channel as Snow", Float) = 0
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
	Fallback "Diffuse"
}