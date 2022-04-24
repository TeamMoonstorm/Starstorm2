Shader "StubbedRoR2/Base/Shaders/PostProcess/HGScopeShader" {
	Properties {
		[Header(Scope Properties)] _ScopeMap ("Scope Distortion (R), Scope Tint (G)", 2D) = "white" {}
		[HideInInspector] _MainTex ("", any) = "" {}
		_Scale ("Scale", Range(1, 10)) = 1
		_DistortionStrength ("Distortion Strength", Range(-1, 1)) = 1
		_TintStrength ("Tint Strength", Range(0, 1)) = 0.5
		[Header(Base Image Properties)] _Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
}