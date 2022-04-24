Shader "StubbedRoR2/Base/Shaders/PostProcess/HGScreenDamage" {
	Properties {
		_Tint ("Vignette Tint", Vector) = (0.5,0.5,0.5,1)
		_NormalMap ("Normal Map Texture", 2D) = "white" {}
		[HideInInspector] _MainTex ("", any) = "" {}
		_TintStrength ("Vignette Strength", Range(0, 5)) = 1
		_DesaturationStrength ("Desaturation Strength", Range(0, 1)) = 1
		_DistortionStrength ("Distortion Strength", Range(0, 1)) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}