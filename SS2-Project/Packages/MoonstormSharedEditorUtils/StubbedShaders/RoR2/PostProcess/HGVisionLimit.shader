Shader "StubbedRoR2/Base/Shaders/PostProcess/HGVisionLimit" {
	Properties {
		_Origin ("Effect Origin", Vector) = (0,0,0,0)
		_RangeEnd ("Range Start", Float) = 0
		_RangeEnd ("Range End", Float) = 1
		_Color ("Tint", Vector) = (1,1,1,1)
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
}