Shader "StubbedRoR2/Base/Shaders/UI/HGUIAnimateAlpha" {
	Properties {
		_MainTex ("Gradient (R) Mask (G)", 2D) = "grey" {}
		_RemapTex ("Color Remap Ramp (RGB)", 2D) = "grey" {}
		_GradientScale ("Gradient Scale", Range(0, 1.5)) = 1
		[Toggle(PINGPONG)] _PingPong ("PingPong Ramp", Float) = 0
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
	Fallback "Transparent/VertexLit"
}