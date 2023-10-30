//////////////////////////////////////////
//
// NOTE: This is *not* a valid shader file
//
///////////////////////////////////////////
Shader "StubbedRoR2/Base/Shaders/SpeedTreeCustom" {
Properties {
_Color ("Main Color", Color) = (1,1,1,1)
_HueVariation ("Hue Variation", Color) = (1,0.5,0,0.1)
_MainTex ("Base (RGB) Trans (A)", 2D) = "white" { }
_DetailTex ("Detail", 2D) = "black" { }
_BumpMap ("Normal Map", 2D) = "bump" { }
_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.333
_Smoothness ("Smoothness", Range(0, 1)) = 0
_SpecularStrength ("Specular Strength", Range(0, 1)) = 0
_SpecularExponent ("Specular Exponent", Range(0.1, 20)) = 1
[Header(Emission Parameters)] [Toggle(USE_EMISSION)] _EmissionOn ("Enable Emission", Float) = 0
_EmissionTex ("Emission Texture", 2D) = "black" { }
_EmissionTint ("Emission Tint", Color) = (0,0,0,0)
[Header(Lighting Parameters)] [MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Float) = 2
[MaterialEnum(None,0,Fastest,1,Fast,2,Better,3,Best,4,Palm,5)] _WindQuality ("Wind Quality", Range(0, 5)) = 0
}

Fallback "Transparent/Cutout/VertexLit"
CustomEditor "SpeedTreeMaterialInspector"
}