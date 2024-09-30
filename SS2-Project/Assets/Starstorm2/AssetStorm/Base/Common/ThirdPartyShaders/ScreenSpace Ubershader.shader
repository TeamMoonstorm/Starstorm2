
// Copyright © 2019 Leviant
// Email: leviant@yandex.ru
// Discord: Leviant#8796
// Discord server: https://discord.gg/MdykFMf
// License: http://opensource.org/licenses/MIT
// GitHub: https://github.com/Leviant/ScreenSpace_Ubershader
// Version: 2.9 (12.11.2019)

// Edited by Aidan_ogg#0001

Shader "Leviant's Shaders/UberShader v2.9"
{
	Properties 
	{
		//Main Settings
		[Toggle(_)]Particle_Render("Main/Setup for Particle system", Int) = 0
		//Fade Settings
		_MinRange ("Main/Start fading", Float) = 2.0
		_MaxRange ("Main/End distance", Float) = 10.0

		//Glitch
		[Toggle(_)]Glitch("Glitch/Active", Int) = 0
		[PowerSlider(2.0)]_Glitch_Intensity("Glitch/Intensity", Range(0, 1)) = 0.1
		_Glitch_BlockSize("Glitch/Block size", Float) = 10.0
		[PowerSlider(2.0)]_Glitch_Macroblock("Glitch/Macroblock subdivide", Range(0, 1)) = 0.3
		[PowerSlider(2.0)]_Glitch_Blocks("Glitch/Block Glitch", Range(0, 1)) = 0.25
		[PowerSlider(2.0)]_Glitch_Lines("Glitch/Line Glitch", Range(0, 1)) = 0.5
		_Glitch_UPS("Glitch/Glitches per second", Float) = 15.0
		[PowerSlider(2.0)]_Glitch_ActiveTime("Glitch/Active Time", Range(0, 1)) = 0.4
		_Glitch_PeriodTime("Glitch/Period Time", Float) = 6.0
		[PowerSlider(2.0)]_Glitch_Duration("Glitch/Long duration chance", Range(0, 1)) = 0.4
		[PowerSlider(2.0)]_Glitch_Displace("Glitch/Displace", Range(0, 1)) = 0.02
		[PowerSlider(2.0)]_Glitch_Pixelization("Glitch/Pixelization", Range(0, 1)) = 0.8
		[PowerSlider(2.0)]_Glitch_Shift("Glitch/Shift", Range(0, 1)) = 0.05
		[PowerSlider(2.0)]_Glitch_Grayscale("Glitch/Grayscale", Range(0, 1)) = 1
		[PowerSlider(2.0)]_Glitch_ColorShift("Glitch/Color shift", Range(0, 1)) = 0.1
		[PowerSlider(2.0)]_Glitch_Interleave("Glitch/Interleave lines", Range(0, 1)) = 0.5
		[PowerSlider(2.0)]_Glitch_BrokenBlock("Glitch/Broken blocks", Range(0, 1)) = 0.05
		[PowerSlider(2.0)]_Glitch_Posterization("Glitch/Posterization", Range(0, 1)) = 0.9
		[PowerSlider(2.0)]_Glitch_Displace_Chance("Glitch/Dispalce chance", Range(0, 1)) = 0.01
		[PowerSlider(2.0)]_Glitch_Pixelization_Chance("Glitch/Pixelization chance", Range(0, 1)) = 1
		[PowerSlider(2.0)]_Glitch_Shift_Chance("Glitch/Shift chance", Range(0, 1)) = 0.05
		[PowerSlider(2.0)]_Glitch_Grayscale_Chance("Glitch/Grayscale chance", Range(0, 1)) = 0.1
		[PowerSlider(2.0)]_Glitch_ColorShift_Chance("Glitch/Color shift chance", Range(0, 1)) = 1
		[PowerSlider(2.0)]_Glitch_Interleave_Chance("Glitch/Interleave lines chance", Range(0, 1)) = 0.05
		[PowerSlider(2.0)]_Glitch_BrokenBlock_Chance("Glitch/Broken block chance", Range(0, 1)) = 1
		[PowerSlider(2.0)]_Glitch_Posterization_Chance("Glitch/Posterization chance", Range(0, 1)) = 1
		//Zoom Settings
		[IntRange]Magnification("Zoom/Mode", Range(0, 6)) = 0
		[PowerSlider(2.0)]_Magnification("Zoom/Scale", Range (-1, 1)) = 0.1
		[PowerSlider(2.0)]_Gravitation("Zoom/Gravitation range", Range (0, 100.0)) = 1.0
		_AngleStartFade("Zoom/Angle range", Range (0, 1)) = 0.25
		_MaxAngle("Zoom/Max angle range", Range (0, 1)) = 0.5
		
		//Girlscam
		_SizeGirls("Girlscam/Size", Range(0, 1)) = 0
		_TimeGirls("Girlscam/Time", Range(0, 1)) = 1

		//Rotation
		[Toggle(_)]ScreenRotation("Rotation/Active", Int) = 0
		_ScreenRotation("Rotation/Angle", Float) = 0.1
		_ScreenRotationSpeed("Rotation/Shake speed", Float) = 2.0

		//Screen Transform
		_ScreenHorizontalFlip("Flip/Horizontal", Range(0.0, 1.0)) = 0
		_ScreenVerticalFlip("Flip/Vertical", Range(0.0, 1.0)) = 0

		//Screen Shake
		[Toggle(_)]Shake("Screen Shake/Active", Int) = 0
		[Normal][NoScaleOffset]_ShakeTex("Screen Shake/Normalmap", 2D) = "bump" {}
		[PowerSlider(2.0)]_SIntensity_X ("Screen Shake/Intensity X", Range(0, 1)) = 0.01
		[PowerSlider(2.0)]_SIntensity_Y ("Screen Shake/Intensity Y", Range(0, 1)) = 0.01
		_ShakeScroll("Screen Shake/Texture Scroll(XY)", Vector) = (2, 0.02, 0, 0)
		_ShakeWave("Screen Shake/Wave offset(XY)", Vector) = (0.01, 0.01, 0, 0)
		_ShakeWaveSpeed("Screen Shake/Wave speed(XY)", Vector) = (20, 19, 0, 0)

		//Pixelation
		[Toggle(_)]Pixelization("Pixelation/Active", Int) = 0
		[PowerSlider(2.0)]_PSize_X ("Pixelation/Pixel Width", Range(1.0, 128.0)) = 4.0
		[PowerSlider(2.0)]_PSize_Y ("Pixelation/Pixel Height", Range(1.0, 128.0)) = 4.0

		//Screen Distortion
		[Toggle(_)]Distorsion("Screen Distorsion/Active", Int) = 0
		[Toggle(_)]Wave_Distorsion("Screen Distorsion/Wave Active", Int) = 1
		[Toggle(_)]Texture_Distorsion("Screen Distorsion/Texture Active", Int) = 0
		[Normal]_DistorsionTex("Screen Distorsion/Normalmap", 2D) = "bump" {}
		[PowerSlider(2.0)]_DIntensity_X ("Screen Distorsion/Horizontal", Range(0, 10)) = 0.01
		[PowerSlider(2.0)]_DIntensity_Y ("Screen Distorsion/Vertical", Range(0, 10)) = 0.01
		_DistorsionScroll("Screen Distorsion/Scroll Texture(XY)", Vector) = (0, 0, 0, 0)
		_DistorsionWave("Screen Distorsion/Wave offset(XYZ)", Vector) = (0.01, 0.01, 1, 0)
		_DistorsionWaveSpeed("Screen Distorsion/Wave speed(XYZ)", Vector) = (2.6, -3.1, 1, 0)
		_DistorsionWaveDensity("Screen Distorsion/Wave density(XYZ)", Vector) = (8.4, 3, 1, 0)
		
		//Blur Settings
		[IntRange]Blur("Blur/Mode", Range(0, 4)) = 0
		[Toggle(_)]Blur_Distorsion("Blur/Blur with Distorsion", Int) = 0
		[Toggle(_)]_Blur_Dithering("Blur/Dithering", Float) = 1
		[HDR]_BlurColor("Blur/Blur Color (RGB)", Color) = (1,1,1,1)
		[PowerSlider(2.0)]_BlurRange ("Blur/Offset", Range(0, 1)) = 0.01
		_BlurRotation ("Blur/Rotation", Float) = 0.0
		_BlurRotationSpeed("Blur/Rotation speed", Float) = 0
		_BlurIterations ("Blur/Samples", Range(1, 128)) = 8.0
		_BlurCenterOffset("Blur/Center offset(XY)", Vector) = (0, 0, 0, 0)
		_BlurMask("Blur/Mask effect", Range(0.0, 1.0)) = 0.5

		//Chromatic Aberration
		[IntRange]Chromatic_Aberration("Chromatic Aberration/Mode", Range(0, 2)) = 0
		[IntRange]Aberration_Quality("Chromatic Aberration/Quality", Range(0, 1)) = 1
		
		[Toggle(_)]CA_Distorsion("Chromatic Aberration/Use Distorsion", Int) = 0
		[Toggle(_)]_CA_dithering("Chromatic Aberration/Dithering", Float) = 1
		[PowerSlider(2.0)]_CA_amplitude("Chromatic Aberration/Offset", Range(0.0, 1.0)) = 0.015
		_CA_iterations ("Chromatic Aberration/Samples", Range(1, 128.0)) = 8.0
		_CA_speed("Chromatic Aberration/Animation Speed", Float) = 0.0
		_CA_direction("Chromatic Aberration/Vector direction", Vector) = (1, 0, 0, 0)
		_CA_factor ("Chromatic Aberration/Effect", Range(0, 1.0)) = 1.0
		_CA_centerOffset("Chromatic Aberration/Radial center offset", Vector) = (0, 0, 0, 0)
		_CA_mask("Chromatic Aberration/Mask effect", Range(0.0, 1.0)) = 0.5

		//Neon
		[Toggle(_)]Neon("Neon/Active", Int) = 0
		[HDR]_NeonColor("Neon/Tint (RGB)", Color) = (1, 1, 1, 1)
		_NeonColorAlpha("Neon/Intensity", Range(0.0, 1.0)) = 1.0
		_NeonOrigColor("Neon/Background Color (RGB)", Color) = (0.25, 0.25, 0.25, 1)
		_NeonOrigColorAlpha("Neon/Background mix", Range(0.0, 1.0)) = 1.0
		_NeonBrightness("Neon/Brightness", Float) = 3.0
		_NeonPosterization("Neon/Posterization", Range (0.0, 1.0)) = 1.0
		_NeonWidth("Neon/Width", Float) = 1.5
		_NeonGlow("Neon/Glow", Range (0.0, 1.0)) = 1.0

		//HSV Colour Space
		[Toggle(_)]HSV_Selection("HSV Selection/Active", Int) = 0
		_TargetColor("HSV Selection/Select color (RGB)", Color) = (1,0,0,1)
		_HueRange("HSV Selection/Hue range", Range(0, 0.5)) = 0.02
		_SaturationRange("HSV Selection/Saturation range", Range(0, 1)) = 0.4
		_LightnessRange("HSV Selection/Lightness range", Range(0, 1)) = 1
		_HueSmoothRange("HSV Selection/Hue fade", Range(0, 0.5)) = 0.02
		_SaturationSmoothRange("HSV Selection/Saturation fade", Range(0, 1)) = 0.1
		_LightnessSmoothRange("HSV Selection/Lightness fade", Range(0, 1)) = 1
		[Toggle(_)]HSV_Desaturate_Selected("HSV Selection/Desaturate", Int) = 1

		//Extra Settings
		[Toggle(_)]HSV_Transform("HSV Transform/Active", Int) = 0
		_TransformColor("HSV Transform/Color (RGB)", Color) = (0, 0, 1, 1)
		_Hue("HSV Transform/Hue value", Range(0, 1)) = 1.0
		_HueAnimationSpeed("HSV Transform/Hue Animation Speed", Float) = 0.0
		_Saturation("HSV Transform/Saturation value", Range(0, 1)) = 0
		_Lightness("HSV Transform/Lightness value", Range(0, 1)) = 0

		//Colour Correction
		[Toggle(_)]Color_Tint("Color Correction/Active", Int) = 0
		[Toggle(_)]ACES_Tonemapping("Color Correction/ACES Tonemapping", Int) = 0
		[HDR]_EmissionColor("Color Correction/Emission color (RGB)", Color) = (0, 0, 0, 1)
		[HDR]_Color("Color Correction/Mix color (RGB)", Color) = (0, 0, 0, 0)
		_ColorAlpha("Color Correction/Mix factor", Range(0.0, 1.0)) = 0.0
		_Grayscale("Color Correction/Grayscale", Range (0.0, 1.0)) = 0.0
		_Contrast("Color Correction/Contrast", Vector) = (1.0, 1.0, 1.0, 1.0)
		_Gamma("Color Correction/Gamma", Vector) = (1.0, 1.0, 1.0, 1.0)
		_Brightness("Color Correction/Brightness", Vector) = (1.0, 1.0, 1.0, 1.0)
		_RedInvert("Color Correction/Red Invert", Range (0.0, 1.0)) = 0.0
		_GreenInvert("Color Correction/Green Invert", Range (0.0, 1.0)) = 0.0
		_BlueInvert("Color Correction/Blue Invert", Range (0.0, 1.0)) = 0.0

		//Posterization
		[Toggle(_)]Posterization("Posterization/Active", Int) = 0
		[PowerSlider(2.0)]_PosterizationSteps("Posterization/Gradient steps", Range(1.0, 256.0)) = 16.0
		
		//Dithering
		[Toggle(_)]Dithering("Dithering/Active", Int) = 0
		[Toggle(_)]Dithering_Colorize("Dithering/Colorize", Int) = 1
		[NoScaleOffset]_DitheringMask("Dithering/Mask", 2D) = "white" {}

		//Overlay Texture
		[Toggle(_)]Overlay_Texture("Overlay/Active", Int) = 0
		[Toggle(_)]Overlay_Grid("Overlay/Image grid", Int) = 1
		_OverlayTex("Overlay/Texture", 2D) = "white" {}
		[HDR]_OverlayTint("Overlay/Tint (RGB)", Color) = (1, 1, 1, 1)
		_OverlayOpaque("Overlay/Opaque", Range(0, 1)) = 0.0
		_OverlayTransparent("Overlay/Transparent", Range(0, 2)) = 1.0
		_OverlayRotation("Overlay/Rotation", Float) = 0
		_OverlayScroll("Overlay/Scroll Vector", Vector) = (0, 1, 0, 0)
		[Toggle(_)]Overlay_Texture_Sheet("Overlay/Texture sheet enable", Int) = 0
		
		_OverlayColumns("Overlay/Columns", Int) = 4
		_OverlayRows("Overlay/Rows", Int) = 4
		_OverlayStartFrame("Overlay/Start frame", Int) = 0
		_OverlayTotalFrames("Overlay/Total frames", Int) = 16
		_OverlayAnimationSpeed("Overlay/Animation speed", Float) = 0.8

		[Toggle(_)]_isGlitchActive("Overlay/Enable Glitch Overlay", Range(0,1)) = 0
		_RGBGlitchBlocksPower("Overlay/Glitch Block Power", Float) = 0.01
		[Toggle(_)]_isRedActive ("Overlay/Toggle Red", Float) = 1
		[Toggle(_)]_isGreenActive ("Overlay/Toggle Green", Float) = 1
		[Toggle(_)]_isBlueActive ("Overlay/Toggle Blue", Float) = 1

		//Static
		[Toggle(_)]Static_Noise("Static noise/Active", Int) = 0
		[HDR]_StaticColour("Static noise/Color", Color) = (1,1,1,1)
		_StaticIntensity("Static noise/Intensity", Range(-1, 1)) = -0.34
		_StaticAlpha("Static noise/Alpha", Range(0,1)) = 0.17
		_StaticBrightness("Static noise/Brightness", Range(0, 1)) = 1.0
		[HideInInspector]_MaskAmount("Static noise/Mix Amount (WIP)", Range(0,1)) = 0

		//Vignette
		[Toggle(_)]Vignette("Vignette/Active", Int) = 0
		_VignetteColor("Vignette/Color (RGB)", Color) = (0, 0, 0, 1)
		_VignetteAlpha("Vignette/Transparent", Range(0, 1)) = 0.15
		_VignetteWidth("Vignette/Width", Float) = 0.5
		_VignetteShape("Vignette/Shape", Range(-1, 1)) = 0.5
		_VignetteRounding("Vignette/Rounding", Range(0, 1)) = 0.5

		//Mask Texture
		[Toggle(_)]Mask_Texture("Mask/Active", Int) = 0
		[Toggle(_)]Mask_Multisampling("Mask/Multisampling", Int) = 0
		[Toggle(_)]Mask_Noise("Mask/Generate Noise", Int) = 0
		_MaskTex("Mask/Texture", 2D) = "white" {}
		[HDR]_MaskColor("Mask/Mix color (RGB)", Color) = (1, 1, 1, 0)
		_MaskAlpha("Mask/Color mix", Range(0, 1)) = 0.0
		_MaskScroll("Mask/Scroll Vector", Vector) = (0, 0, 0, 0)

		[Header(Shader by Leviant#8796)]
		[HideInInspector] _Info("Discord server https://discord.gg/MdykFMf", Int) = 20191211
	}
	CustomEditor "LeviantScreenSpaceEditor"
	SubShader 
	{
		Tags { "Queue"="Overlay+2" "RenderType"="Overlay" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "PreviewType" = "None"}
		ZWrite Off
		ZTest Off
		Cull Off
		GrabPass { "_UberShaderGrabTexture" }
		Pass 
		{
			CGPROGRAM
			//#pragma enable_d3d11_debug_symbols
			#pragma target 5.0 //use full instruction set DX11 
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#define MAGNIFICATION_SIMPLE_SCALE 1 
			#define MAGNIFICATION_ZOOM 2 
			#define MAGNIFICATION_ZOOM_FALLOFF 3 
			#define MAGNIFICATION_CENTERING 4
			#define MAGNIFICATION_GRAVITATIONAL_LENS 5
			#define MAGNIFICATION_DEPTH_ZOOM 6

			#define BLUR_HORIZONTAL 1
			#define BLUR_STAR 2
			#define BLUR_CIRCLE 3
			#define BLUR_RADIAL 4

			#define CHROMATIC_ABERRATION_VECTOR 1
			#define CHROMATIC_ABERRATION_RADIAL 2

			#define ABERRATION_QUALITY_SIMPLE_SPLIT 0
			#define ABERRATION_QUALITY_MULTISAMPLING 1

			struct appdata
			{
				float4 vertex : POSITION;
				float3 center : TEXCOORD0; //xyz - Particle world position center
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 grabPos : TEXCOORD0;
				
				float4 uv : TEXCOORD1;     //[0 .. 1] overlayScreenSpace
				float3 worldRayDir: TEXCOORD2;
				float3 viewPos : TEXCOORD3;
				nointerpolation float3 viewCenter : TEXCOORD4;
				nointerpolation float falloff : TEXCOORD5;  //[0 .. 1] shader effect strength
				nointerpolation float2 grabPosForward : TEXCOORD6; // view center position in grabTexture (not for single pass stereo instancing)
				UNITY_VERTEX_OUTPUT_STEREO
			};
			uniform int Particle_Render;
			uniform int Magnification;
			uniform int ScreenRotation;
			uniform int Shake;
			uniform int Pixelization;
			uniform int Distorsion;
			uniform int Wave_Distorsion;
			uniform int Texture_Distorsion;
			uniform int Blur;
			uniform int Blur_Distorsion;
			uniform int Chromatic_Aberration;
			uniform int Aberration_Quality;
			uniform int CA_Distorsion;
			uniform int Neon;
			uniform int HSV_Selection;
			uniform int HSV_Desaturate_Selected;
			uniform int HSV_Transform;
			uniform int Color_Tint;
			uniform int ACES_Tonemapping;
			uniform int Posterization;
			uniform int Dithering;
			uniform int Dithering_Colorize;
			uniform int Overlay_Texture;
			uniform int Overlay_Grid;
			uniform int Overlay_Texture_Sheet;
			uniform int Glitch;
			uniform int Vignette;
			uniform int Static_Noise;

			uniform SamplerState linear_mirror_sampler;
			#define grabSampler linear_mirror_sampler

#ifdef UNITY_STEREO_INSTANCING_ENABLED
			Texture2DArray _UberShaderGrabTexture;
			Texture2DArray _CameraDepthTexture;
			float4 SampleGrabTexture(float2 uv)
			{
				return _UberShaderGrabTexture.SampleLevel(grabSampler, float3(uv, unity_StereoEyeIndex), 0);
			}
			float4 SampleDepthTexture(float2 uv)
			{
				return _CameraDepthTexture.SampleLevel(grabSampler, float3(uv, unity_StereoEyeIndex), 0);
			}
#else
			Texture2D _UberShaderGrabTexture;
			Texture2D _CameraDepthTexture;
			float4 SampleGrabTexture(float2 uv)
			{
				return _UberShaderGrabTexture.SampleLevel(grabSampler, uv, 0);
			}
			float4 SampleDepthTexture(float2 uv)
			{
				return _CameraDepthTexture.SampleLevel(grabSampler, uv, 0);
			}
#endif
			uniform float4 _UberShaderGrabTexture_TexelSize;

#ifdef UNITY_SINGLE_PASS_STEREO
			static const float2 factor = float2(0.5, 1.0);
#else
			static const float2 factor = float2(1.0, 1.0);
#endif
			static const uint LeftEye = 0;
			static const uint RightEye = 1;
			
			//Falloff
			uniform float _MinRange;
			uniform float _MaxRange;

			uniform float _Glitch_Intensity;
			uniform float _Glitch_BlockSize;
			uniform float _Glitch_UPS;
			uniform float _Glitch_Macroblock;
			uniform float _Glitch_Blocks;
			uniform float _Glitch_Lines;
			uniform float _Glitch_ActiveTime;
			uniform float _Glitch_PeriodTime;
			uniform float _Glitch_Duration;
			uniform float _Glitch_Displace;
			uniform float _Glitch_Pixelization;
			uniform float _Glitch_Shift;
			uniform float _Glitch_Grayscale;
			uniform float _Glitch_ColorShift;
			uniform float _Glitch_Interleave;
			uniform float _Glitch_BrokenBlock;
			uniform float _Glitch_Posterization;

			uniform float _Glitch_Displace_Chance;
			uniform float _Glitch_Pixelization_Chance;
			uniform float _Glitch_Shift_Chance;
			uniform float _Glitch_Grayscale_Chance;
			uniform float _Glitch_ColorShift_Chance;
			uniform float _Glitch_Interleave_Chance;
			uniform float _Glitch_BrokenBlock_Chance;
			uniform float _Glitch_Posterization_Chance;

			uniform float _Magnification;
			uniform float _Gravitation;
			uniform float _AngleStartFade;
			uniform float _MaxAngle;

			uniform float _SizeGirls;
			uniform float _TimeGirls;

			uniform sampler2D _DistorsionTex;
			uniform float4 _DistorsionTex_ST;
			uniform float4 _DistorsionTex_TexelSize;
			uniform float2 _DistorsionScroll;
			uniform float3 _DistorsionWave;
			uniform float3 _DistorsionWaveSpeed;
			uniform float3 _DistorsionWaveDensity;
			uniform float _DIntensity_X;
			uniform float _DIntensity_Y;

			uniform float _PSize_X;
			uniform float _PSize_Y;

			uniform float _PosterizationSteps;

			uniform Texture2D _DitheringMask;
			uniform float4 _DitheringMask_TexelSize;

			uniform float _ScreenRotation;
			uniform float _ScreenRotationSpeed;

			uniform sampler2D _ShakeTex;
			uniform float4 _ShakeTex_ST;
			uniform float2 _ShakeWave;
			uniform float2 _ShakeWaveSpeed;
			uniform float2 _ShakeScroll;
			uniform float _SIntensity_X;
			uniform float _SIntensity_Y;

			uniform float _ScreenHorizontalFlip;
			uniform float _ScreenVerticalFlip;

			uniform float _CA_dithering;
			uniform float _CA_amplitude;
			uniform float _CA_speed;
			uniform float _CA_iterations;
			uniform float _CA_factor;
			uniform float _CA_mask;
			uniform float2 _CA_direction;
			uniform float2 _CA_centerOffset;

			uniform float _Blur_Dithering;
			uniform float _BlurRange;
			uniform float _BlurIterations;
			uniform float _BlurRotation;
			uniform float _BlurRotationSpeed;
			uniform float _BlurMask;
			uniform float2 _BlurCenterOffset;
			uniform float3 _BlurColor;

			uniform float3 _EmissionColor;
			uniform float3 _Color;
			uniform float3 _Contrast;
			uniform float3 _Gamma;
			uniform float3 _Brightness;
			uniform float _ColorAlpha;
			uniform float _Grayscale;
			uniform float _RedInvert;
			uniform float _GreenInvert;
			uniform float _BlueInvert;

			uniform float3 _TargetColor;
			uniform float _HueRange;
			uniform float _SaturationRange;
			uniform float _LightnessRange;
			uniform float _HueSmoothRange;
			uniform float _SaturationSmoothRange;
			uniform float _LightnessSmoothRange;

			uniform float3 _TransformColor;
			uniform float _HueAnimationSpeed;
			uniform float _Hue;
			uniform float _Saturation;
			uniform float _Lightness;

			uniform float3 _NeonColor;
			uniform float3 _NeonOrigColor;
			uniform float _NeonColorAlpha;
			uniform float _NeonOrigColorAlpha;
			uniform float _NeonBrightness;
			uniform float _NeonPosterization;
			uniform float _NeonWidth;
			uniform float _NeonGlow;

			uniform sampler2D _OverlayTex;
			uniform float4 _OverlayTex_ST;
			uniform float2 _OverlayScroll;
			uniform float3 _OverlayTint;
			uniform float _OverlayOpaque;
			uniform float _OverlayTransparent;
			uniform float _OverlayRotation;
			uniform uint _OverlayColumns;
			uniform uint _OverlayRows;
			uniform uint _OverlayStartFrame;
			uniform uint _OverlayTotalFrames;
			uniform float _OverlayAnimationSpeed;
			uniform uint _isGlitchActive;
			uniform float _RGBGlitchBlocksPower;
			uniform uint _isRedActive;
			uniform uint _isGreenActive;
			uniform uint _isBlueActive;

			uniform float _MaskAmount;
			uniform float _StaticAlpha;
			uniform float _StaticBrightness;
			uniform float _StaticIntensity;
			uniform float3 _StaticColour;

			uniform float3 _VignetteColor;
			uniform float _VignetteAlpha;
			uniform float _VignetteWidth;
			uniform float _VignetteShape;
			uniform float _VignetteRounding;

			uniform int Mask_Texture;
			uniform int Mask_Multisampling;
			uniform int Mask_Noise;
			uniform sampler2D _MaskTex;
			uniform float4 _MaskTex_ST;
			uniform float3 _MaskColor;
			uniform float2 _MaskScroll;
			uniform float _MaskAlpha;

			bool IsInMirror() //Thanks DocMe ^w^
			{
				return unity_CameraProjection[2][0] != 0.0 || unity_CameraProjection[2][1] != 0.0;
			}

			float3 sqr(float3 x)
			{
				return x * x;
			}
		#if defined(UNITY_SINGLE_PASS_STEREO)
			float2 TransformStereoScreenSpaceTex2(float2 uv, float w, uint eye)
			{
				float4 scaleOffset = unity_StereoScaleOffset[eye];
				return uv.xy * scaleOffset.xy + scaleOffset.zw * w;
			}
		#else
			#define TransformStereoScreenSpaceTex2(uv, w, eye) uv
		#endif

			inline float4 ComputeGrabScreenPos2(float4 pos, uint eye) 
			{
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
				float4 o = pos * 0.5f;
				o.xy = float2(o.x, o.y * scale) + o.w;
			#ifdef UNITY_SINGLE_PASS_STEREO
				o.xy = TransformStereoScreenSpaceTex2(o.xy, pos.w, eye);
			#endif
				o.zw = pos.zw;
				return o;
			}
			v2f vert(appdata v) 
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			#ifdef USING_STEREO_MATRICES
				float3 nonStereoCameraPosition = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1])*0.5;
			#else
				float3 nonStereoCameraPosition = _WorldSpaceCameraPos;
			#endif
				float4 viewPos = float4(v.vertex.xyz, 1);
				float3 worldCenter;
				if(Particle_Render)
				{
					viewPos.xyz -= v.center;
					worldCenter = v.center;
				}
				else
				{
					worldCenter = mul(unity_ObjectToWorld, float4(0,0,0,1));
				}
				float dist = distance(nonStereoCameraPosition, worldCenter);
				UNITY_BRANCH if(dist > _MaxRange || IsInMirror())
				{
					o.pos = float4(0, 0, -2, 1);
					return o;
				}
				float4 clipPos = UnityViewToClipPos(viewPos);
				o.grabPos = ComputeGrabScreenPos(clipPos);
				float4 grabPosFocus = ComputeGrabScreenPos(UnityViewToClipPos(float4(0, 0, 1, 1)));
				grabPosFocus /= grabPosFocus.w;
				o.grabPosForward = grabPosFocus.xy;

				float4 viewPos2 = viewPos;

				o.falloff = smoothstep(1, 0, (dist - _MinRange) / (_MaxRange - _MinRange));
				float3 viewCenter = UnityWorldToViewPos(worldCenter);
				o.viewCenter = viewCenter;
				o.viewPos = viewPos;
				
				[forcecase] switch(Magnification)
				{
				case MAGNIFICATION_SIMPLE_SCALE:
					_Magnification = 2.0 - 2.0 / (_Magnification + 1.0);
					o.grabPos.xy = lerp(o.grabPos.xy, grabPosFocus.xy * o.grabPos.w , o.falloff * _Magnification);
					break;
				case MAGNIFICATION_ZOOM:
					{
						float4 clipCenter = UnityViewToClipPos(viewCenter);
						float4 grabPosCenter = ComputeGrabScreenPos(clipCenter);
				
						float borderFalloff = grabPosCenter.w > 0 ? 1 : 0;
						float2 grabPosCenterNormalized = grabPosCenter.xy / max(0.0001, grabPosCenter.w);
						float4 uv = ComputeNonStereoScreenPos(clipCenter);
						uv /= uv.w;
						borderFalloff *= step(0, uv.x)*step(0, uv.y)*step(-1, -uv.x)*step(-1, -uv.y); // check in range [-1 .. 1]
						_Magnification = 2.0 - 2.0 / (_Magnification + 1.0);
						o.grabPos.xy = lerp(o.grabPos.xy, grabPosCenterNormalized * o.grabPos.w, borderFalloff * o.falloff * _Magnification);
						break;
					}
				case MAGNIFICATION_ZOOM_FALLOFF:
					{
						float angle = 1 - acos(dot(normalize(worldCenter - nonStereoCameraPosition), UNITY_MATRIX_V[2])) / UNITY_PI;
						float linearRange = (angle - _AngleStartFade) / (_MaxAngle - _AngleStartFade);
						float angleFalloff = smoothstep(1, 0, linearRange);

						float4 clipCenter = UnityViewToClipPos(viewCenter);
						float4 grabPosCenter = ComputeGrabScreenPos(clipCenter);
						float2 grabPosCenterNormalized = grabPosCenter / grabPosCenter.w;
						_Magnification = 2.0 - 2.0 / (_Magnification + 1.0);
						o.grabPos.xy = lerp(o.grabPos.xy, grabPosCenterNormalized * o.grabPos.w, angleFalloff * o.falloff * _Magnification);
						break;
					}
				case MAGNIFICATION_CENTERING:
					{
						float3 v_forward = normalize(-viewCenter);
						float angle = 1 - acos(dot(normalize(worldCenter - nonStereoCameraPosition), UNITY_MATRIX_V[2])) / UNITY_PI;
						float linearRange = (angle - _AngleStartFade) / (_MaxAngle - _AngleStartFade);
						float angleFalloff = smoothstep(1, 0, linearRange);

						v_forward = lerp(float3(0, 0, 1), v_forward, angleFalloff * o.falloff);
						float3 v_up = float3(0, 1, 0);
						float3 v_right = -normalize(cross(v_forward, v_up));
						v_up = -normalize(cross(v_right, v_forward));
				
						float3x3 matrix_v = float3x3(v_right, v_up, v_forward);
						viewPos2.xyz = mul(matrix_v, viewPos2.xyz);

						float4 clipCenter = UnityViewToClipPos(viewCenter);
						float4 grabPosCenter = ComputeGrabScreenPos(clipCenter);
						float2 grabPosCenterNormalized = grabPosCenter / grabPosCenter.w;

						o.grabPos.xy = lerp(o.grabPos.xy, grabPosCenterNormalized * o.grabPos.w, angleFalloff * o.falloff * _Magnification);
						break;
					}
				}

				UNITY_BRANCH if(ScreenRotation)
				{
					float s, c;
					sincos(_ScreenRotation*cos(_Time.y*_ScreenRotationSpeed*UNITY_TWO_PI)*o.falloff, s, c);
					viewPos2.xy = mul(float2x2(c, -s, s, c), viewPos2.xy);
				}

				o.grabPos.xy = lerp(o.grabPos.xy, o.grabPos.ww - o.grabPos.xy, float2(_ScreenHorizontalFlip, _ScreenVerticalFlip) * o.falloff);
				UNITY_BRANCH if(Shake)
				{
					float2 shake = UnpackNormal(tex2Dlod(_ShakeTex, float4(_Time.x * _ShakeScroll.xy, 0, 0)));
					shake *= float2(_SIntensity_X, _SIntensity_Y);
					shake += _ShakeWave.xy * float2(cos(_Time.y * _ShakeWaveSpeed.x), sin(_Time.y * _ShakeWaveSpeed.y));
					shake.x *= _ScreenParams.y / _ScreenParams.x;
					o.grabPos.xy += o.grabPos.w * shake * o.falloff * factor.x;
				}
				o.worldRayDir = mul((float3x3)UNITY_MATRIX_I_V, viewPos2);
				o.pos = UnityViewToClipPos(viewPos2);
				o.uv = ComputeNonStereoScreenPos(o.pos);

				return o;
			}

			// Hash without Sine
			// https://www.shadertoy.com/view/4djSRW

			static const float4 hashScaleSmall = float4(443.8975, 397.2973, 491.1871, 444.129);
			static const float4 hashScale = float4(0.1031, 0.1030, 0.0973, 0.1099);

			float hash11(float p)
			{
				p = frac(p * hashScale.x);
				p *= p + 33.33;
				p *= p + p;
				return frac(p);
			}
			float hash11s(float p)
			{
				p = frac(p * hashScaleSmall.x);
				p *= p + 33.33;
				p *= p + p;
				return frac(p);
			}
			float hash12(float2 p)
			{
				float3 p3  = frac(float3(p.xyx) * hashScale.x);
				p3 += dot(p3, p3.yzx + 19.19);
				return frac((p3.x + p3.y) * p3.z);
			}
			float hash12s(float2 p)
			{
				float3 p3 = frac(float3(p.xyx) * hashScaleSmall.x);
				p3 += dot(p3, p3.yzx + 19.19);
				return frac((p3.x + p3.y) * p3.z);
			}
			float hash13(float3 p3)
			{
				p3  = frac(p3 * hashScale.x);
				p3 += dot(p3, p3.yzx + 19.19);
				return frac((p3.x + p3.y) * p3.z);
			}
			float hash13s(float3 p3)
			{
				p3 = frac(p3 * hashScaleSmall.x);
				p3 += dot(p3, p3.yzx + 19.19);
				return frac((p3.x + p3.y) * p3.z);
			}
			float2 hash21(float p)
			{
				float3 p3 = frac(p * hashScale.xyz);
				p3 += dot(p3, p3.yzx + 19.19);
				return frac((p3.xx + p3.yz) * p3.zy);

			}
			float2 hash21s(float p)
			{
				float3 p3 = frac(p * hashScaleSmall.xyz);
				p3 += dot(p3, p3.yzx + 19.19);
				return frac((p3.xx + p3.yz) * p3.zy);

			}
			float2 hash22(float2 p)
			{
				float3 p3 = frac(float3(p.xyx) * hashScale.xyz);
				p3 += dot(p3, p3.yzx+19.19);
				return frac((p3.xx+p3.yz)*p3.zy);
			}
			float2 hash22s(float2 p)
			{
				float3 p3 = frac(float3(p.xyx) * hashScaleSmall.xyz);
				p3 += dot(p3, p3.yzx + 19.19);
				return frac((p3.xx + p3.yz) * p3.zy);
			}
			float2 hash23(float3 p3)
			{
				p3 = frac(p3 * hashScale.xyz);
				p3 += dot(p3, p3.yzx + 33.33);
				return frac((p3.xx + p3.yz) * p3.zy);
			}
			float3 hash31(float p)
			{
				float3 p3 = frac(p * hashScale.xyz);
				p3 += dot(p3, p3.yzx + 33.33);
				return frac((p3.xxy + p3.yzz) * p3.zyx);
			}
			float3 hash32(float2 p)
			{
				float3 p3 = frac(p.xyx * hashScale.xyz);
				p3 += dot(p3, p3.yxz + 33.33);
				return frac((p3.xxy + p3.yzz) * p3.zyx);
			}
			float3 hash33(float3 p3)
			{
				p3 = frac(p3 * hashScale.xyz);
				p3 += dot(p3, p3.yxz+19.19);
				return frac((p3.xxy + p3.yxx)*p3.zyx);
			}
			float4 hash41(float p)
			{
				float4 p4 = frac(p * hashScale);
				p4 += dot(p4, p4.wzxy + 33.33);
				return frac((p4.xxyz + p4.yzzw) * p4.zywx);
			}
			float4 hash42(float2 p)
			{
				float4 p4 = frac(p.xyxy * hashScale);
				p4 += dot(p4, p4.wzxy + 33.33);
				return frac((p4.xxyz + p4.yzzw) * p4.zywx);
			}
			float4 hash43(float3 p)
			{
				float4 p4 = frac(float4(p.xyzx)  * hashScale);
				p4 += dot(p4, p4.wzxy+19.19);
				return frac((p4.xxyz+p4.yzzw)*p4.zywx);
			}
			float4 hash44(float4 p4)
			{
				p4 = frac(p4 * hashScale);
				p4 += dot(p4, p4.wzxy + 33.33);
				return frac((p4.xxyz + p4.yzzw) * p4.zywx);
			}
			float4 hash44s(float4 p4)
			{
				p4 = frac(p4 * hashScaleSmall);
				p4 += dot(p4, p4.wzxy + 33.33);
				return frac((p4.xxyz + p4.yzzw) * p4.zywx);
			}
			uint lcg_rand(inout uint4 seed)
			{
				seed = seed * 1664525 + 1013904223UL;
				return seed;
			}
			uint rand_xorshift(inout uint rng_state)
			{
				// Xorshift algorithm from George Marsaglia's paper
				rng_state ^= (rng_state << 13);
				rng_state ^= (rng_state >> 17);
				rng_state ^= (rng_state << 5);
				return rng_state;
			}
			uint rng_xor128(inout uint4 state)
			{
				uint t = state.x ^ (state.x << 11);
				state.xyz = state.yzw;
				state.w = (state.w ^ (state.w >> 19)) ^ (t ^ (t >> 8));
				return state.w;
			}
			float mod(float x, float y)
			{
				return frac(x / y)*y;
			}
			float2 mod(float2 x, float2 y)
			{
				return frac(x / y)*y;
			}

			float mask1(float2 uv)
			{
				if(Mask_Texture)
				{
					float maskColor;
					if(Mask_Noise)
						maskColor = hash13(float3(uv, _Time.x));
					else
					{
						maskColor = tex2D(_MaskTex, TRANSFORM_TEX(uv, _MaskTex) + _Time.y*_MaskScroll.xy);
						if(Mask_Multisampling > 0)
							for(float a = 0.5; a >= 0.125; a /= 2)
							{
								float2 maskUV = TRANSFORM_TEX(uv, _MaskTex) + _Time.y*_MaskScroll.xy;
								maskColor = lerp(maskColor, tex2D(_MaskTex, maskUV / a), a);
							}
					}
					return lerp(maskColor, _MaskColor, _MaskAlpha);
				}
				else
					return 1.0;
			}

			float4 mask4(float2 uv)
			{
				if(Mask_Texture)
				{
					float4 maskColor;
					if(Mask_Noise)
						maskColor = hash43(float3(uv, _Time.y));
					else
					{
						maskColor = tex2D(_MaskTex, TRANSFORM_TEX(uv, _MaskTex) + _Time.y*_MaskScroll.xy);
						if(Mask_Multisampling)
							for(float a = 0.5; a >= 0.125; a /= 2)
							{
								float2 maskUV = TRANSFORM_TEX(uv, _MaskTex) + _Time.y*_MaskScroll.xy;
								maskColor = lerp(maskColor, tex2D(_MaskTex, maskUV / a), a);
							}
					}
					return lerp(maskColor, float4(_MaskColor, 1.0), _MaskAlpha);
				}
				else
					return 1.0;
			}
		
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}

			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			// ACES Filmic Tone Mapping Curve
			//
			// Adapted from code by Krzysztof Narkowicz
			// https://knarkowicz.wordpress.com/2016/01/06/
			// aces-filmic-tone-mapping-curve/

			float3 ACESFilm( float3 color )
			{
				float a = 2.51f;
				float b = 0.03f;
				float c = 2.43f;
				float d = 0.59f;
				float e = 0.14f;
				return saturate((color*(a*color+b))/(color*(c*color+d)+e));
			}

			inline float CheckGrabTextureBorder(float2 uv)
			{
			#ifdef UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
				return step(scaleOffset.z, uv.x)*step(0, uv.y)*step(-scaleOffset.z - scaleOffset.x, -uv.x)*step(-1, -uv.y);
			#else
				return step(0, uv.x)*step(0, uv.y)*step(-1, -uv.x)*step(-1, -uv.y);
			#endif
			}

			//return color from red to blue
			float3 rainbowOld(float t)
			{
				float3 dist = 1.0 - 2.0*abs(t - float3(0.0, 0.5, 1.0));
				return max(0, dist*float3(4.0, 2.0, 4.0));
			}

			//https://www.shadertoy.com/view/ls2Bz1
			float3 rainbowJET(float x)
			{
				return float3(4.0*x - 2.0,
    						  4.0*x + min(0.0, 4.0 - 8.0*x),
							  1.0 + 4.0*(0.25-x));
			}
			// --- Spectral Zucconi --------------------------------------------
			// By Alan Zucconi
			// Based on GPU Gems: https://developer.nvidia.com/sites/all/modules/custom/gpugems/books/GPUGems/gpugems_ch08.html
			// But with values optimised to match as close as possible the visible spectrum
			// Fits this: https://commons.wikimedia.org/wiki/File:Linear_visible_spectrum.svg
			// With weighter MSE (RGB weights: 0.3, 0.59, 0.11)
			float3 bump3y (float3 x, float3 yoffset)
			{
				float3 y = float3(1.,1.,1.) - x * x;
				y = saturate(y-yoffset);
				return y;
			}
			float3 rainbow (float x)  //spectral_zucconi
			{
				const float3 cs = float3(3.54541723, 2.86670055, 2.29421995);
				const float3 xs = float3(0.69548916, 0.49416934, 0.28269708);
				const float3 ys = float3(0.02320775, 0.15936245, 0.53520021);

				return bump3y (cs * (x - xs), ys) * float3(2.5, 2.5, 5);
			}
			// --- Spectral Zucconi 6 --------------------------------------------

			// Based on GPU Gems
			// Optimised by Alan Zucconi
			float3 rainbow2 (float x)  //spectral_zucconi6
			{
				const float3 c1 = float3(3.54585104, 2.93225262, 2.41593945);
				const float3 x1 = float3(0.69549072, 0.49228336, 0.27699880);
				const float3 y1 = float3(0.02312639, 0.15225084, 0.52607955);

				const float3 c2 = float3(3.90307140, 3.21182957, 3.96587128);
				const float3 x2 = float3(0.11748627, 0.86755042, 0.66077860);
				const float3 y2 = float3(0.84897130, 0.88445281, 0.73949448);

				float3 result = bump3y(c1 * (x - x1), y1) +
								bump3y(c2 * (x - x2), y2) ;
				return result * float3(2.6, 2.7, 4.6);
			}

			float2 DistorsionSample(float2 uv)
			{
				float2 distorsion = 0;
				if(Texture_Distorsion)
				{
					float2 disUV = uv;
					disUV.x *= _ScreenParams.x / _ScreenParams.y;
					disUV = TRANSFORM_TEX(disUV, _DistorsionTex) + _Time.x * _DistorsionScroll.xy;
					distorsion = UnpackNormal(tex2D(_DistorsionTex, disUV)) * float2(_DIntensity_X, _DIntensity_Y);
					distorsion.x *= _ScreenParams.y/_ScreenParams.x;
				}
				if(Wave_Distorsion)
				{
					float4 sc;
					sincos(_DistorsionWaveDensity.xy*(uv.xy - 0.5) + _Time.yy*_DistorsionWaveSpeed.xy, sc.yw, sc.xz);
					distorsion += _DistorsionWave.xy*(sc.xy + sc.zw);
					//distorsion += _DistorsionWave.xy*cos(length(_DistorsionWaveDensity.xy*(uv - 0.5)) + _Time.y*_DistorsionWaveSpeed.xy);
				}
				return distorsion;
			}
			float3 TriPlanarSample(in float3 position, in float3 normal)
			{
				normal *= normal;
				float3 distorsion = normal.x * UnpackNormal(tex2D(_DistorsionTex, TRANSFORM_TEX(position.zy, _DistorsionTex) + _Time.x * _DistorsionScroll.xy)) +
					normal.y * UnpackNormal(tex2D(_DistorsionTex, TRANSFORM_TEX(position.xz, _DistorsionTex) + _Time.x * _DistorsionScroll.xy)) +
					normal.z * UnpackNormal(tex2D(_DistorsionTex, TRANSFORM_TEX(position.xy, _DistorsionTex) + _Time.x * _DistorsionScroll.xy));
				return distorsion;
			}
			float3 DistorsionSampleDir(in float3 direction)
			{
				float3 distorsion = 0;
				if (Texture_Distorsion)
				{
					float3 position = direction;
					//disUV.x *= _ScreenParams.x / _ScreenParams.y;
					//position = TRANSFORM_TEX(position.xy, _DistorsionTex) + _Time.x * _DistorsionScroll.xy;
					distorsion = TriPlanarSample(position, direction) * float3(_DIntensity_X, _DIntensity_Y, _DIntensity_X);
					
				}
				if (Wave_Distorsion)
				{
					float3 s, c;
					sincos(_DistorsionWaveDensity.xyz * direction.xyz + _Time.yyy * _DistorsionWaveSpeed.xyz, s, c);
					distorsion += _DistorsionWave.xyz * (s + c);
					//distorsion += _DistorsionWave.xy*cos(length(_DistorsionWaveDensity.xy*(uv - 0.5)) + _Time.y*_DistorsionWaveSpeed.xy);
				}
				return distorsion;
			}

			float2 BlurDistorsion(float2 uv) 
			{
				return Blur_Distorsion ? DistorsionSample(uv) : 0;
			}

			//Cheap Cubemap
			//https://www.shadertoy.com/view/ltl3D8
			//returns 2D UV 
			float2 cubeUV(in float3 d)
			{
				// intersect cube
				float3 n = abs(d);
				float3 v = (n.x > n.y && n.x > n.z) ? d.xyz :
					(n.y > n.x && n.y > n.z) ? d.yzx :
					d.zxy;
				// project into face
				float2 q = v.yz / v.x;
				// undistort in the edges
				q *= 1.25 - 0.25 * q * q;

				return 0.5 + 0.5 * q;
			}

			float4 frag(v2f i) : COLOR 
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				float2 vr_uv = i.viewPos.xy / -i.viewPos.z;
				float2 grabPos = i.grabPos.xy / i.grabPos.w;
				float2 grabPosOffsetR = 0;
				float2 grabPosOffsetB = 0;
				i.uv /= i.uv.w;
				i.viewPos = normalize(i.viewPos);
				i.worldRayDir = normalize(i.worldRayDir);
				UNITY_BRANCH if (Glitch)
				{
					float2 uv = grabPos.xy - i.grabPosForward.xy;
					uv /= factor;
					uv.x *= _ScreenParams.x / _ScreenParams.y;
					uv += 1.0;
					//float2 uv = i.viewPos.xy / abs(i.viewPos.z);
					//float2 uv = cubeUV(i.worldRayDir);
					float2 block = floor(uv * _Glitch_BlockSize);
					float linePos = block.y;

					float time = floor(fmod(_Time.y * _Glitch_UPS, 600));
					float rand = hash11(time);
					float2 blockID = block + rand;
					float lineID = linePos + rand;

					const uint max_subdiv = 2;
					float blockSize = _Glitch_BlockSize;
					float lineSize = _Glitch_BlockSize;
					for (uint s = max_subdiv; s; s--)
					{
						float h = hash11s(lineID);
						if (h < _Glitch_Macroblock)
						{
							lineSize *= 2;
							lineID = floor(uv.y * lineSize) + rand + 0.01;
							//lineID += h * linePos;
						}
					}

					for (s = max_subdiv; s; s--)
					{
						float2 h = hash22s(blockID);
						if (h.x < _Glitch_Macroblock)
						{
							blockSize *= 2;
							blockID = floor(uv * blockSize) + rand + 0.01;
							//blockID += h * block;
						}
					}

					/*
					float upsL = _Glitch_UPS;
					float upsB = _Glitch_UPS;
					for (s = max_subdiv; s; s--)
					{
						float2 h = hash22s(blockID);
						if (h.x < _Glitch_Duration)
						{
							upsB *= 2;
							blockID += h * floor(fmod(_Time.y * upsB, 600));
						}
						float h2 = hash11s(lineID);
						if (h2 < _Glitch_Duration)
						{
							upsL *= 2;
							lineID += h2 * floor(fmod(_Time.y * upsL, 600));
						}
					}*/

					float4 line_noise0 = hash41(lineID);
					float4 line_noise1 = hash41(lineID + 1);
					float4 line_noise2 = hash41(lineID + 2);
					float4 block_noise0 = hash42(blockID);
					float4 block_noise1 = hash42(blockID + 1);
					float4 block_noise2 = hash42(blockID + 2);

					_Glitch_ActiveTime = max(0.0001, _Glitch_ActiveTime);
					float glitchMul = pow(abs(sin(_Time.y * UNITY_PI / _Glitch_PeriodTime)), 1.0 / _Glitch_ActiveTime - 1.0);
					float block_thresh = _Glitch_Blocks * glitchMul;
					float line_thresh = _Glitch_Lines * glitchMul;
					line_thresh *= line_thresh;
					/*
					float4 strength[2] = { 
						float4(_Glitch_Displace, _Glitch_Pixelization, _Glitch_Shift, _Glitch_Grayscale),
						float4(_Glitch_ColorShift, _Glitch_Interleave, _Glitch_BrokenBlock, _Glitch_Posterization) 
					};
					float4 lineChances[2] = {
						float4(_Glitch_Displace_Chance, _Glitch_Pixelization_Chance, _Glitch_Shift_Chance, _Glitch_Grayscale_Chance),
						float4(_Glitch_ColorShift_Chance, _Glitch_Interleave_Chance, _Glitch_BrokenBlock_Chance, _Glitch_Posterization_Chance)
					};
					float4 blockChances[2];
					strength[0] *= _Glitch_Intensity;
					strength[1] *= _Glitch_Intensity;
					blockChances[0] = lineChances[0];
					blockChances[1] = lineChances[1];
					blockChances[0] *= block_thresh;
					blockChances[1] *= block_thresh;
					lineChances[0] *= line_thresh;
					lineChances[1] *= line_thresh;*/
					
					_Glitch_Intensity *= i.falloff;
					_Glitch_Displace *= _Glitch_Intensity;	
					_Glitch_Pixelization *= _Glitch_Intensity;
					_Glitch_Shift *= _Glitch_Intensity;
					_Glitch_Grayscale *= _Glitch_Intensity;
					_Glitch_ColorShift *= _Glitch_Intensity;
					_Glitch_Interleave *= _Glitch_Intensity;
					_Glitch_BrokenBlock *= _Glitch_Intensity;
					_Glitch_Posterization *= _Glitch_Intensity;
					
					// Displace
					if (line_noise0.x < line_thresh * _Glitch_Displace_Chance)
					{
						uv.x += (line_noise0.y - 0.5) * _Glitch_Displace;
					}
					else if (block_noise0.x < block_thresh * _Glitch_Displace_Chance)
					{
						uv.xy += (block_noise0.yz - 0.5) * _Glitch_Displace;
					}

					// Pixelization
					if (line_noise1.z < line_thresh * _Glitch_Pixelization_Chance)
					{
						float2 size = exp2(floor(8 - line_noise1.w *  _Glitch_Pixelization * 8));
						uv = (round(uv * lineSize * size + 0.5) - 0.5) / size / lineSize;
					}
					else if (block_noise1.z < block_thresh * _Glitch_Pixelization_Chance)
					{
						float2 size = exp2(floor(8 - block_noise1.w *  _Glitch_Pixelization * 8));
						uv = (round(uv * blockSize * size + 0.5) - 0.5) / size / blockSize;
					}

					// Shift
					if (line_noise0.y < line_thresh * _Glitch_Shift_Chance)
					{
						uv.x += (uv.y - (linePos + 1.0) / lineSize) * (line_noise0.w * 2 - 1) * _Glitch_Shift * lineSize;
					}

					// Grayscale
					if (line_noise0.z < line_thresh * _Glitch_Grayscale_Chance || block_noise0.z < block_thresh * _Glitch_Grayscale_Chance)
					{
						Color_Tint = 1;
						_Grayscale = line_noise0.w * _Glitch_Grayscale;
					}

					// Color Shift
					if (line_noise0.w < line_thresh * _Glitch_ColorShift_Chance)
					{
						grabPosOffsetR.x = (line_noise1.x * 3 - 1.5) * _Glitch_ColorShift * 0.5 * factor;
						grabPosOffsetB.x = (line_noise1.y * 3 - 1.5) * _Glitch_ColorShift * 0.5 * factor;
					}
					else if (block_noise0.w < block_thresh * _Glitch_ColorShift_Chance)
					{
						grabPosOffsetR.xy = (block_noise1.xy - 0.5) * _Glitch_ColorShift * 0.5 * factor;
						grabPosOffsetB.xy = (block_noise1.zw - 0.5) * _Glitch_ColorShift * 0.5 * factor;
					}

					// Interleave
					if (line_noise1.x < line_thresh * _Glitch_Interleave_Chance || block_noise1.x < block_thresh * _Glitch_Interleave_Chance)
					{
						Color_Tint = 1;

						float _line = frac(i.pos.y / 3.0);
						float3 mask;
						if (_line > 2.0 / 3.0)
							mask = float3(0, 0, 3);
						else if (_line > 1.0 / 3.0)
							mask = float3(0, 3, 0);
						else
							mask = float3(3, 0, 0);
						_Brightness *= lerp(1, mask, sqrt(_Glitch_Interleave));
					}
					
					//BrokenBlock
					if (line_noise1.y < line_thresh * _Glitch_BrokenBlock_Chance)
					{
						Color_Tint = 1;
						_Brightness = step(_Glitch_BrokenBlock, line_noise2.xyz);
					}
					else if (block_noise1.y < block_thresh * _Glitch_BrokenBlock_Chance)
					{
						Color_Tint = 1;
						_Brightness = step(_Glitch_BrokenBlock, block_noise2.xyz);
					}

					// Posterization
					if (line_noise1.w < line_thresh * _Glitch_Posterization_Chance)
					{
						Posterization = 1;
						_PosterizationSteps = (1 - _Glitch_Posterization) * lerp(32, 255, line_noise2.x) + 1;
					}
					else if (block_noise1.w < block_thresh * _Glitch_Posterization_Chance)
					{
						Posterization = 1;
						_PosterizationSteps = (1 -_Glitch_Posterization) * lerp(32, 255, block_noise2.x) + 1;
					}
					uv -= 1.0;
					uv.x /= _ScreenParams.x / _ScreenParams.y;
					uv *= factor;
					grabPos.xy = uv + i.grabPosForward.xy;
				}
				UNITY_BRANCH if (_SizeGirls)
				{
					//Girlscam https://www.shadertoy.com/view/4tVcRW
					float scanLineJitter = _SizeGirls * lerp(abs(_SinTime.w), 1.0, sin(_TimeGirls));
					float sl_thresh = saturate(1.0 - scanLineJitter * 1.2);
					float sl_disp = 0.002 + pow(scanLineJitter, 3.0) * 0.05;
					float jitter = hash12s(float2(round(grabPos.y * _UberShaderGrabTexture_TexelSize.w) * _UberShaderGrabTexture_TexelSize.y, _SinTime.w)) * 2.0 - 1.0;
					jitter *= step(sl_thresh, abs(jitter)) * sl_disp;
					grabPos.x += jitter * i.falloff * factor.x;
				}
				UNITY_BRANCH if(Magnification == MAGNIFICATION_GRAVITATIONAL_LENS) 
				{
					float magFalloff = dot(i.viewCenter, i.viewCenter);
					magFalloff = 1.0 / (1.0/ _Gravitation*magFalloff + 1.0);

					i.viewCenter = normalize(i.viewCenter);
					float angle = acos(dot(i.viewPos, i.viewCenter)) / UNITY_PI;
					float3 viewPos = i.viewPos * 0.5;
					
					float angleFalloff = smoothstep(0, 1, (angle - _MaxAngle) / (_AngleStartFade - _MaxAngle));
				
					float3 vec = i.viewCenter - viewPos;
					float3 viewSpaceDistorsion = viewPos + vec * magFalloff * angleFalloff * i.falloff * tan(_Magnification * UNITY_HALF_PI);
					float4 grabPosDest = ComputeGrabScreenPos(UnityViewToClipPos(viewPos));
					float4 grabPosSrc = ComputeGrabScreenPos(UnityViewToClipPos(viewSpaceDistorsion));
					float4 magOffset = grabPosSrc / grabPosSrc.w - grabPosDest / grabPosDest.w;
					grabPos.xy += magOffset.xy;
				}

				float4 maskColor = 1.0;
				float maskGray = 1.0;
				
				UNITY_BRANCH if(Mask_Texture)
				{
					UNITY_BRANCH if(Mask_Noise)
					{
						maskColor.rgb = hash33(float3(vr_uv, frac(_Time.y)));
						maskColor.a  = 1;
					} else {
						maskColor = tex2D(_MaskTex, TRANSFORM_TEX(vr_uv, _MaskTex) + _Time.y*_MaskScroll.xy);
						if(Mask_Multisampling)
						{
							for(float a = 0.5; a >= 0.125; a /= 2)
							{
								float2 maskUV = TRANSFORM_TEX(vr_uv, _MaskTex) + _Time.y*_MaskScroll.xy;
								maskColor = lerp(maskColor, tex2D(_MaskTex, maskUV / a), a);
							}
							maskColor = saturate(maskColor);
						}
					}
					maskColor = lerp(maskColor, float4(_MaskColor, 1.0), _MaskAlpha);
					maskGray = dot(maskColor.rgb, 0.333);
				}
		
				UNITY_BRANCH if(Pixelization)
				{
					float2 blockSize = _ScreenParams.xy / factor / lerp(1.0, float2(_PSize_X, _PSize_Y), i.falloff);
					grabPos.xy = round(grabPos.xy * blockSize) / blockSize;
				}
				
				UNITY_BRANCH if(Distorsion)
				{
					float3 displace = DistorsionSampleDir(i.worldRayDir);

					displace.x *= _ScreenParams.y / _ScreenParams.x;
					grabPos.xy += displace.xy * i.falloff * factor * maskGray;
					//grabPos.xy += DistorsionSample(vr_uv + 0.5) * i.falloff * factor * maskGray;
				}

				float4 color = SampleGrabTexture(grabPos);
				UNITY_BRANCH if (Glitch)
				{
					color.r = SampleGrabTexture(grabPos + grabPosOffsetR).r;
					color.b = SampleGrabTexture(grabPos + grabPosOffsetB).b;
				}
				
				UNITY_BRANCH if(Blur)
				{
					float2 blurVector;
					float3 blurColor = 0;
					float blurStep = 1.0 / _BlurIterations;
					float dithering = blurStep * _Blur_Dithering * (fmod(i.pos.x + i.pos.y * 2, 4) / 4 - 0.5);
					//float dithering = blurStep * hash12(i.pos.xy);
					[forcecase] switch (Blur)
					{
						case BLUR_HORIZONTAL:
						{
							sincos((_BlurRotation + _Time.y * _BlurRotationSpeed) * UNITY_HALF_PI, blurVector.y, blurVector.x);
							blurVector.y *= _ScreenParams.x / _ScreenParams.y;
							blurVector *= _BlurRange;
							//blurVector += BlurDistorsion(vr_uv);
							UNITY_BRANCH if (Blur_Distorsion)
								blurVector += DistorsionSampleDir(i.worldRayDir).xy;
							blurVector *= i.falloff * factor;

							for (float x = -0.5; x <= 0.5; x += blurStep)
								blurColor += SampleGrabTexture(grabPos.xy + (x + dithering) * blurVector);
							break;
						}
						case BLUR_STAR:
						{
							sincos((_BlurRotation + _Time.y * _BlurRotationSpeed) * UNITY_HALF_PI, blurVector.y, blurVector.x);
							blurVector.y *= _ScreenParams.x / _ScreenParams.y;
							blurVector *= _BlurRange;
							//blurVector += BlurDistorsion(vr_uv);
							UNITY_BRANCH if (Blur_Distorsion)
								blurVector += DistorsionSampleDir(i.worldRayDir).xy;
							blurVector *= i.falloff * factor;

							for (float x = -0.5; x <= 0.5; x += blurStep)
								blurColor += SampleGrabTexture(grabPos.xy + (x + dithering) * blurVector);
							sincos((_BlurRotation + 1 + _Time.y * _BlurRotationSpeed) * UNITY_HALF_PI, blurVector.y, blurVector.x);
							blurVector.y *= _ScreenParams.x / _ScreenParams.y;
							blurVector *= _BlurRange;
							//blurVector += BlurDistorsion(vr_uv);
							UNITY_BRANCH if (Blur_Distorsion)
								blurVector += DistorsionSampleDir(i.worldRayDir).xy;
							blurVector *= i.falloff * factor;
							for (x = -0.5; x <= 0.5; x += blurStep)
								blurColor += SampleGrabTexture(grabPos.xy + (x + dithering) * blurVector);
							blurColor *= 0.5;
							break;
						}
						case BLUR_CIRCLE:
						{
							float rotation = (_BlurRotation + _Time.y * _BlurRotationSpeed) * UNITY_HALF_PI;
							float2 vecMul = _BlurRange;
							vecMul.y *= _ScreenParams.x / _ScreenParams.y;
							//vecMul += BlurDistorsion(vr_uv);
							UNITY_BRANCH if (Blur_Distorsion)
								vecMul += DistorsionSampleDir(i.worldRayDir).xy;
							vecMul *= i.falloff * factor;
							for (float x = 0.0; x <= 1.0; x += blurStep)
							{
								sincos(rotation + (x + dithering) * UNITY_TWO_PI, blurVector.y, blurVector.x);
								blurVector *= vecMul;
								blurColor += SampleGrabTexture(grabPos.xy + blurVector);
							}
							break;
						}
						case BLUR_RADIAL:
						{
							blurVector = -(vr_uv + _BlurCenterOffset.xy);
							float2 rotatedBlurVec = float2(blurVector.y, -blurVector.x);
							float2 sc;
							sincos((_BlurRotation + _Time.y * _BlurRotationSpeed) * UNITY_HALF_PI, sc.y, sc.x);
							blurVector = blurVector * sc.x + rotatedBlurVec * sc.y;
							blurVector.y *= _ScreenParams.x / _ScreenParams.y;
							blurVector *= _BlurRange;
							//blurVector += BlurDistorsion(vr_uv);
							UNITY_BRANCH if (Blur_Distorsion)
								blurVector += DistorsionSampleDir(i.worldRayDir).xy;
							blurVector *= i.falloff * factor;

							for (float x = -0.5; x <= 0.5; x += blurStep)
								blurColor += SampleGrabTexture(grabPos.xy + (x + dithering) * blurVector);
							break;
						}
					}
					blurColor /= floor(_BlurIterations + 1.0);
					color.rgb = lerp(color.rgb, blurColor.rgb, _BlurColor * lerp(1.0, maskGray, _BlurMask));
				}
				UNITY_BRANCH if(Chromatic_Aberration)
				{
					float shift = 0.5 + 0.5*cos(_Time.y*_CA_speed);
					shift = _CA_amplitude * pow(shift, 3.0) * i.falloff;
					float2 shift2;
					if(Chromatic_Aberration >= CHROMATIC_ABERRATION_RADIAL)
					{
						#ifdef UNITY_SINGLE_PASS_STEREO
							shift2 = vr_uv + _CA_centerOffset.xy;
						#else
							shift2 = vr_uv * _ScreenParams.yy / _ScreenParams.xy + _CA_centerOffset.xy;
						#endif
					
						float l = length(shift2);
						shift2 *= -shift*l;
					}
					else
					{
						shift2 = _CA_direction.xy * shift;
						shift2.y *= _ScreenParams.x / _ScreenParams.y;
					}
					shift2 *= factor;
					float3 chromatcColor = 0;

					UNITY_BRANCH if(Aberration_Quality >= ABERRATION_QUALITY_MULTISAMPLING)
					{
						float w = 1.0 / _CA_iterations;
						//float dithering = w * (hash12(i.pos.xy) - 0.5);
						float dithering = w * _CA_dithering * (fmod(i.pos.x + i.pos.y * 2, 4) / 4 - 0.5);
						UNITY_BRANCH if(CA_Distorsion)
							shift2 += DistorsionSampleDir(i.worldRayDir).xy * i.falloff;
						UNITY_BRANCH if(Chromatic_Aberration <= CHROMATIC_ABERRATION_VECTOR)
							UNITY_LOOP for(float t = 0.0; t < 1.0; t += w)
							{
								float2 uv = (t - 0.5 + dithering) * shift2;
								chromatcColor += rainbow(t) * SampleGrabTexture(grabPos.xy + uv).rgb;
							}
						else
							UNITY_LOOP for(float x = 0.0; x < 1.0 ; x += w)
							{
								float2 uv = (x + dithering) * shift2;
								chromatcColor += rainbow(x) * SampleGrabTexture(grabPos.xy + uv).rgb;
							}
						chromatcColor /= _CA_iterations;
						color.rgb = lerp(color.rgb, chromatcColor, _CA_factor * lerp(1.0, maskColor.rgb, _CA_mask));
					}
					else
					{
						UNITY_BRANCH if(CA_Distorsion)
							shift2 += DistorsionSampleDir(i.worldRayDir).xy * i.falloff;
						color.r = lerp(color.r, SampleGrabTexture(grabPos - shift2).r, _CA_factor* lerp(1.0, maskColor.r, _CA_mask));
						color.b = lerp(color.b, SampleGrabTexture(grabPos + shift2).b, _CA_factor* lerp(1.0, maskColor.b, _CA_mask));
					}
				}

				UNITY_BRANCH if(Neon)
				{
					float3 pix = float3(_UberShaderGrabTexture_TexelSize.xy, 0) * _NeonWidth * factor.x;
					float3 color_sub = SampleGrabTexture(grabPos.xy - pix.xz);
					color_sub += SampleGrabTexture(grabPos.xy + pix.xz);
					color_sub += SampleGrabTexture(grabPos.xy - pix.zy);
					color_sub += SampleGrabTexture(grabPos.xy + pix.zy);
					float3 delta_color = 4.0*color.rgb*_NeonOrigColorAlpha - color_sub;
					delta_color *= _NeonBrightness;
					delta_color = lerp(delta_color, length(delta_color) > 1.0 ? normalize(delta_color) : 0.0, _NeonPosterization);
					delta_color = lerp(delta_color, abs(delta_color), _NeonGlow);
					float3 neonColor = color.rgb * _NeonOrigColor.rgb + delta_color *_NeonColor.rgb;
					color.rgb = lerp(color.rgb, neonColor, _NeonColorAlpha * i.falloff);
				}
				float3 hsv;
				float hsvMask;
				UNITY_BRANCH if (HSV_Selection || HSV_Transform)
					hsv = RGBToHSV(color.rgb);
				else
					hsv = 0.0;
				UNITY_BRANCH if (HSV_Selection)
				{
					float3 targetHSV = RGBToHSV(_TargetColor.rgb);
					float3 diff;
					diff.x = frac(targetHSV.x - hsv.x);
					diff.x -= step(0.5, diff.x);
					diff.yz = targetHSV.yz - hsv.yz;

					hsvMask = abs(diff.x) < _HueRange ? 1 : 1 - saturate((abs(diff.x) - _HueRange) / _HueSmoothRange - 1);
					hsvMask *= diff.y < _SaturationRange ? 1 : 1 - saturate(max(0, diff.y - _SaturationRange) / _SaturationSmoothRange - 1);
					hsvMask *= diff.z < _LightnessRange ? 1 : 1 - saturate(max(0, diff.z - _LightnessRange) / _LightnessSmoothRange - 1);

					if (HSV_Desaturate_Selected)
						hsv.y = hsv.y * hsvMask;
					if (HSV_Transform == 0)
						color.rgb = lerp(color.rgb, HSVToRGB(hsv), i.falloff);
				}
				else
					hsvMask = 1.0;

				UNITY_BRANCH if(HSV_Transform)
				{
					float3 transformHSV = RGBToHSV(_TransformColor.rgb);
					transformHSV.x = frac(transformHSV.x + _Time.y * _HueAnimationSpeed);
					if (_Hue < 1.0)
					{
						float hue_diff = frac(transformHSV.x - hsv.x);
						hue_diff -= step(0.5, hue_diff);
						float hue_shift = -8.0*hue_diff*(hue_diff*hue_diff - 0.25); //Smoothing hue shift
						hsv.x = frac(hsv.x + hue_shift * _Hue);
					}
					else
						hsv.x = transformHSV.x;
					hsv.y = lerp(hsv.y, transformHSV.y, _Saturation);
					hsv.z = lerp(hsv.z, transformHSV.z, _Lightness);
					color.rgb = lerp(color.rgb, HSVToRGB(hsv), i.falloff * hsvMask);
				}

				UNITY_BRANCH if(Color_Tint)
				{
					float3 col = color;
					if(ACES_Tonemapping)
						col = ACESFilm(max(0, col));
					col = lerp(col, 1 - min(1, col), float3(_RedInvert, _GreenInvert, _BlueInvert));
					col = max(0.0, (col - 0.5)*_Contrast + 0.5);
					col = pow(col, _Gamma);
					col *= _Brightness;
					col = lerp(col, LinearRgbToLuminance(col), _Grayscale);
					col = lerp(col, _Color, _ColorAlpha);
					col += _EmissionColor.rgb;
					color.rgb = lerp(color.rgb, col, i.falloff);
				}

				UNITY_BRANCH if(Posterization)
				{
					float luminance = max(0.001, LinearRgbToLuminance(color.rgb));
					if(Dithering)
					{
						// 8x8 ordered dithering, texture-based
						float dither = _DitheringMask[uint2(fmod(i.pos.xy, _DitheringMask_TexelSize.zw))];
						dither = pow(dither, 1 / 2.4); //Gamma Correction
						if (Dithering_Colorize)
						{
							// Dithering each color
							float3 c = color.rgb * _PosterizationSteps;
							c = floor(c) + step(dither, frac(c));
							color.rgb = lerp(color.rgb, c / _PosterizationSteps, i.falloff);
						}
						else 
						{
							// Dithering by luminance
							float floorLum = floor(luminance * _PosterizationSteps) / _PosterizationSteps;
							float ceilLum = ceil(luminance * _PosterizationSteps) / _PosterizationSteps;
							float dtLum = luminance * _PosterizationSteps - floor(luminance * _PosterizationSteps);
							color.rgb = lerp(color.rgb, color.rgb / luminance * (dtLum > dither ? ceilLum : floorLum), i.falloff);
						}
					}
					else
						color.rgb = lerp(color.rgb, color.rgb / luminance * round( luminance * _PosterizationSteps ) / _PosterizationSteps, i.falloff);
				}

				UNITY_BRANCH if(Overlay_Texture)
				{
					float s, c;
					sincos(_OverlayRotation * UNITY_HALF_PI, s, c);
				#ifdef UNITY_SINGLE_PASS_STEREO
					// Calculate UV for VR
					float2 overlayUV = vr_uv;
				#else
					// Moving uv origin to center
					i.uv.xy -= 0.5;
					// Screen aspect correction
					i.uv.xy *= _ScreenParams.xy / _ScreenParams.yy;
					float2 overlayUV = i.uv.xy;
				#endif
					//float2 overlayUV = vr_uv;
					// Rotate UV
					overlayUV = mul(float2x2(c, -s, s, c), overlayUV);
					// Move uv origin back
					overlayUV = TRANSFORM_TEX(overlayUV, _OverlayTex) + 0.5 + _Time.y * _OverlayScroll.xy;

					float overlayAlpha = 1.0;
					

					UNITY_BRANCH if (Overlay_Texture_Sheet)
					{
						if (Overlay_Grid)
							overlayUV = frac(overlayUV);
						else
						{
							overlayAlpha = 0.0 <= overlayUV.x && overlayUV.x <= 1.0 && 0.0 <= overlayUV.y && overlayUV.y <= 1.0;
							overlayUV = saturate(overlayUV);
						}
						uint frame = floor(_Time.y * _OverlayAnimationSpeed * _OverlayTotalFrames);
						uint current = (_OverlayStartFrame + frame) % _OverlayTotalFrames;
						uint2 spritePosition = uint2(current % _OverlayColumns, _OverlayRows - 1 - current / _OverlayColumns);

						overlayUV = (overlayUV + spritePosition) / uint2(_OverlayColumns, _OverlayRows);
					}
					else if (Overlay_Grid == 0)
					{
						overlayAlpha = 0.0 <= overlayUV.x && overlayUV.x <= 1.0 && 0.0 <= overlayUV.y && overlayUV.y <= 1.0;
						overlayUV = saturate(overlayUV);
					}
					float4 overlaySample = 0;
					UNITY_BRANCH if (_isGlitchActive)
					{
						if (_isRedActive)
							overlaySample.ra = max(overlaySample.ra, tex2D(_OverlayTex, float4(overlayUV + (hash21s(_SinTime.x) * 2 - 1) * _RGBGlitchBlocksPower, 0, 0)).ra);
						if (_isGreenActive)
							overlaySample.ga = max(overlaySample.ga, tex2D(_OverlayTex, float4(overlayUV + (hash21s(_SinTime.y) * 2 - 1) * _RGBGlitchBlocksPower, 0, 0)).ga);
						if (_isBlueActive)
							overlaySample.ba = max(overlaySample.ba, tex2D(_OverlayTex, float4(overlayUV + (hash21s(_CosTime.x) * 2 - 1) * _RGBGlitchBlocksPower, 0, 0)).ba);
					}
					else
						overlaySample = tex2Dlod(_OverlayTex, float4(overlayUV, 0, 0));
					overlayAlpha *= overlaySample.a;
					
					UNITY_BRANCH if (_OverlayOpaque < 0.999)
						overlayAlpha = saturate((overlayAlpha + _OverlayTransparent - 1) / (1 - _OverlayOpaque));
					else
						overlayAlpha = step(1 - _OverlayTransparent, overlayAlpha);
					color.rgb = lerp(color.rgb, _OverlayTint * overlaySample.rgb, overlayAlpha * i.falloff);
				}
				
				UNITY_BRANCH if (Static_Noise)
				{
					//float staticValue = hash13(float3(grabPos.xy, _Time.x));
					//staticValue *= staticValue <= _StaticIntensity ? _StaticBrightness / _StaticIntensity : 0.0;
					//float3 staticColor = lerp(color.rgb, _StaticColour.rgb, saturate(staticValue + _StaticAlpha - 1));

					float staticValue = hash13s(float3(grabPos.xy, _Time.x)) * tan(_StaticIntensity * UNITY_HALF_PI);
					float3 staticColor = staticValue * _StaticColour.rgb * lerp(color.rgb, 1.0, _StaticAlpha);
					color.rgb = lerp(color.rgb, staticColor + color.rgb * _StaticBrightness, i.falloff);
				}
				UNITY_BRANCH if(Vignette)
				{
					float2 VignetteUV = vr_uv;
					if (_VignetteShape < 0)
						VignetteUV.y *= 1.0 + _VignetteShape;
					else
						VignetteUV.x *= 1.0 - _VignetteShape;

					_VignetteRounding = max(0.05, _VignetteRounding);
					float z = pow(dot(1.0, pow(abs(VignetteUV), 2.0 / _VignetteRounding)), _VignetteRounding / 2.0);
					float vignette = saturate(pow(saturate(z + _VignetteWidth), 1.0 / tan(_VignetteAlpha * UNITY_HALF_PI)));

					color.rgb = lerp(color.rgb, _VignetteColor, vignette * i.falloff);
					
					//The best formula for desktop (but not for VR) from https://www.shadertoy.com/view/lsKSWR
					//float2 windowPos = VignetteUV;
					//windowPos *= 1.0 - windowPos.yx;
					//float vignette = windowPos.x * windowPos.y * (1.0 - _VignetteAlpha) * 100.0;
					//color.rgb = lerp(color.rgb, _VignetteColor, saturate(1.0 - pow(vignette, _VignetteWidth)) * i.falloff);
				}

				return color;
			} //frag
			ENDCG
		} //Pass
	} //SubShader
}  //Shader
