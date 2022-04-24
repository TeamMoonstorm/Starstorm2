Shader "StubbedRoR2/Base/Shaders/Deferred/HGStandard" {
	Properties {
		[Header(Default)] [Toggle(CUTOUT)] _EnableCutout ("Cutout", Float) = 0
		_Color ("Main Color", Vector) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB) Specular Scale (A)", 2D) = "white" {}
		_NormalStrength ("Normal Strength", Range(0, 5)) = 1
		_NormalTex ("Normal Map", 2D) = "bump" {}
		_EmColor ("Emission Color", Vector) = (0,0,0,1)
		[NoScaleOffset] _EmTex ("Emission Tex (RGB)", 2D) = "white" {}
		_EmPower ("Emission Power", Range(0, 10)) = 1
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		[Toggle(FORCE_SPEC)] _ForceSpecOn ("Ignore Diffuse Alpha for Speculars", Float) = 0
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		[MaterialEnum(Default,0,Environment,1,Character,2,Misc,3)] _DecalLayer ("Decal Layer", Float) = 0
		_SpecularStrength ("Specular Strength", Range(0, 1)) = 0
		_SpecularExponent ("Specular Exponent", Range(0.1, 20)) = 1
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Float) = 2
		[Header(Screenspace Dithering)] [Toggle(DITHER)] _DitherOn ("Enable Dither", Float) = 0
		_FadeBias ("Fade Bias", Range(0, 1)) = 0
		[Header(Fresnel Emission)] [Toggle(FRESNEL_EMISSION)] _FEON ("Enable Fresnel Emission", Float) = 0
		[NoScaleOffset] _FresnelRamp ("Fresnel Ramp", 2D) = "white" {}
		_FresnelPower ("Fresnel Power", Range(0.1, 20)) = 1
		[NoScaleOffset] _FresnelMask ("Fresnel Mask", 2D) = "white" {}
		_FresnelBoost ("Fresnel Boost", Range(0, 20)) = 1
		[Header(Print Behavior)] [Toggle(PRINT_CUTOFF)] _PrintOn ("Enable Printing", Float) = 0
		_SliceHeight ("Slice Height", Range(-25, 25)) = 5
		_SliceBandHeight ("Print Band Height", Range(0, 10)) = 1
		_SliceAlphaDepth ("Print Alpha Depth", Range(0, 1)) = 0.1
		_SliceAlphaTex ("Print Alpha Texture", 2D) = "gray" {}
		_PrintBoost ("Print Color Boost", Range(0, 10)) = 1
		_PrintBias ("Print Alpha Bias", Range(0, 4)) = 0
		_PrintEmissionToAlbedoLerp ("Print Emission to Albedo Lerp", Range(0, 1)) = 0
		[MaterialEnum(BottomUp,0,TopDown,1,BackToFront,3)] _PrintDirection ("Print Direction", Float) = 0
		[NoScaleOffset] _PrintRamp ("Print Ramp", 2D) = "gray" {}
		[Header(Elite Remap Behavior)] [PerRendererData] _EliteIndex ("Elite Index", Float) = 0
		_EliteBrightnessMin ("Elite Brightness, Min", Range(-10, 10)) = 0
		_EliteBrightnessMax ("Elite Brightness, Max", Range(-10, 10)) = 1
		[Header(Splatmapping)] [Toggle(SPLATMAP)] _SplatmapOn ("Enable Splatmap", Float) = 0
		[Toggle(USE_VERTEX_COLORS)] _ColorsOn ("Use Vertex Colors Instead", Float) = 0
		_Depth ("Blend Depth", Range(0, 1)) = 0.2
		_SplatmapTex ("Splatmap Tex (RGB)", 2D) = "white" {}
		_SplatmapTileScale ("Splatmap Tile Scale", Range(0, 20)) = 1
		[NoScaleOffset] _GreenChannelTex ("Green Channel Albedo (RGB) Specular Scale (A)", 2D) = "white" {}
		[NoScaleOffset] _GreenChannelNormalTex ("Green Channel Normal Map", 2D) = "bump" {}
		_GreenChannelSmoothness ("Green Channel Smoothness", Range(0, 1)) = 0
		_GreenChannelBias ("Green Channel Bias", Range(-2, 5)) = 0
		[NoScaleOffset] _BlueChannelTex ("Blue Channel Albedo  Specular Scale (A)", 2D) = "white" {}
		[NoScaleOffset] _BlueChannelNormalTex ("Blue Channel Normal Map", 2D) = "bump" {}
		_BlueChannelSmoothness ("Blue Channel Smoothness", Range(0, 1)) = 0
		_BlueChannelBias ("Blue Channel Bias", Range(-2, 5)) = 0
		[Header(Flowmap)] [Toggle(FLOWMAP)] _FlowmapOn ("Enable Flowmap", Float) = 0
		[NoScaleOffset] _FlowTex ("Flow Vector (RG), Noise (B)", 2D) = "bump" {}
		_FlowHeightmap ("Flow Heightmap", 2D) = "white" {}
		_FlowHeightRamp ("Flow Ramp", 2D) = "black" {}
		_FlowHeightBias ("Flow Height Bias", Range(-1, 1)) = 0
		_FlowHeightPower ("Flow Height Power", Range(0.1, 20)) = 1
		_FlowEmissionStrength ("Flow Height Strength", Range(0.1, 20)) = 1
		_FlowSpeed ("Flow Speed", Range(0, 100)) = 1
		_FlowMaskStrength ("Mask Flow Strength", Range(0, 5)) = 0
		_FlowNormalStrength ("Normal Flow Strength", Range(0, 5)) = 1
		_FlowTextureScaleFactor ("Flow Texture Scale Factor", Range(0, 10)) = 1
		[Header(Limb Removal)] [Toggle(LIMBREMOVAL)] _LimbRemovalOn ("Enable Limb Removal", Float) = 0
		[PerRendererData] _LimbPrimeMask ("Limb Prime Mask", Range(1, 10000)) = 1
		[PerRendererData] _FlashColor ("Flash Color", Vector) = (0,0,0,1)
		[PerRendererData] _Fade ("Fade", Range(0, 1)) = 1
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