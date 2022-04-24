Shader "StubbedCalm Water/Shaders/ProjectorCaustics" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_CausticTex ("Cookie", 2D) = "" {}
		_Speed ("Caustic Speed", Float) = 1
		_Tiling ("Tiling", Float) = 1
		_FalloffTex ("FallOff", 2D) = "" {}
		_Size ("Size", Float) = 5
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