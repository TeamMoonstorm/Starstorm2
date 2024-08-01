Shader "StubbedTextMeshPro/Distance Field"
{
    Properties
    {
        _FaceTex ("Face Texture", 2D) = "white" {}
        _FaceUVSpeedX ("Face UV Speed X", Range(-5,5)) = 0
        _FaceUVSpeedY ("Face UV Speed Y", Range(-5,5)) = 0
        [HDR] _FaceColor ("Face Color", Color) = (1,1,1,1)
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0
        [HDR] _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineTex ("Outline Texture", 2D) = "white" {}
        _OutlineUVSpeedX ("Outline UV Speed X", Range(-5, 5)) = 0
        _OutlineUVSpeedY ("Outline UV Speed Y", Range(-5, 5)) = 0
        _OutlineWidth ("Outline Thickness", Range(0,1)) = 0
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0
        _Bevel ("Bevel", Range(0,1)) = .5
        _BevelOffset ("Bevel Offset", Range(-.5, .5)) = 0
        _BevelWidth ("Bevel Width", Range(-.5, .5)) = 0
        _BevelClamp ("Bevel Clamp", Range(0,1)) = 0
        _BevelRoundness ("Bevel Roundness", Range(0,1)) = 0
        _LightAngle ("Light Angle", Range(0, 6.28)) = 3.1416
        [HDR] _SpecularColor ("Specular", Color) = (1,1,1,1)
        _SpecularPower ("Specular", Range(0,4)) = 2
        _Reflectivity ("Reflectivity", Range(5, 15)) = 10
        _Diffuse ("Diffuse", Range(0, 1)) = .5
        _Ambient ("Ambient", Range(0, 1)) = .5
        _BumpMap ("Normal Map", 2D) = "white" {}
        _BumpOutline ("Bump Outline", Range(0, 1)) = 0
        _BumpFace ("Bump Face", Range(0, 1)) = 0
        _ReflectFaceColor ("Reflection Color", Color) = (0,0,0,1)
        _ReflectOutlineColor ("Reflection Color", Color) = (0,0,0,1)
        _Cube ("Reflection Cubemap", 2D) = "white" {}
        _EnvMatrixRotation ("Texture Rotation", Vector) = (0,0,0,0)
        [HDR] _UnderlayColor ("Border Color", Color) = (0,0,0,0.5)
        _UnderlayOffsetX ("Border OffsetX", Range(-1,1)) = 0
        _UnderlayOffsetY ("Border OffsetY", Range(-1, 1)) = 0
        _UnderlayDilate ("Border Dilate", Range(-1, 1)) = 0
        _UnderlaySoftness ("Border Softness", Range(0, 1)) = 0
        [HDR] _GlowColor ("Color", Color) = (0, 1, 0, .5)
        _GlowOffset ("Offset", Range(-1,1)) = 0
        _GlowInner ("Inner", Range(0, 1)) = 0.05
        _GlowOuter ("AmbOuterient", Range(0, 1)) = 0.05
        _GlowPower ("Falloff", Range(1, 0)) = 0.75
        _WeightNormal ("Weight Normal", Float) = 0
        _WeightBold ("Weight Bold", Float) = 0.5
        _ShaderFlags ("Flags", Float) = 0
        _ScaleRatioA ("Scale RatioA", Float) = 1
        _ScaleRatioB ("Scale RatioB", Float) = 1
        _ScaleRatioC ("Scale RatioC", Float) = 1
        _MainTex ("Font Atlas", 2D) = "white" {}
        _TextureWidth ("Texture Width", Float) = 512
        _TextureHeight ("Texture Height", Float) = 512
        _GradientScale ("Gradient Scale", Float) = 5
        _ScaleX ("Scale X", Float) = 1
        _ScaleY ("Scale Y", Float) = 1
        _PerspectiveFilter ("Perspective Correction", Range(0,1)) = 0.875
        _Sharpness ("Sharpness", Range(-1,1)) = 0
        _VertexOffsetX ("Vertex OffsetX", Float) = 0
        _VertexOffsetY ("Vertex OffsetY", Float) = 0
        _MaskCoord ("Mask Coordinates", Vector) = (0,0,32767,32767)
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
        _MaskSoftnessX ("Mask SoftnessX", Float) = 0
        _MaskSoftnessY ("Mask SoftnessY", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _CullMode ("Cull Mode", Float) = 0
        _ColorMask ("Color Mask", Float) = 15
    }
    FallBack "Diffuse"
}
