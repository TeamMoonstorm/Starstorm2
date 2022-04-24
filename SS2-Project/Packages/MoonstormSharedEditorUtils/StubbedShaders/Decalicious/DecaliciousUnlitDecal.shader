Shader "StubbedDecalicious/Shader/DecaliciousUnlitDecal.shader" {
	Properties {
		_MaskTex ("Mask", 2D) = "white" {}
		[PerRendererData] _MaskMultiplier ("Mask (Multiplier)", Float) = 1
		_MainTex ("Albedo", 2D) = "white" {}
		[HDR] _Color ("Albedo (Multiplier)", Vector) = (1,1,1,1)
		_DecalBlendMode ("Blend Mode", Float) = 0
		_DecalSrcBlend ("SrcBlend", Float) = 1
		_DecalDstBlend ("DstBlend", Float) = 10
		_AngleLimit ("Angle Limit", Float) = 0.5
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
	//CustomEditor "ThreeEyedGames.DecalShaderGUI"
}