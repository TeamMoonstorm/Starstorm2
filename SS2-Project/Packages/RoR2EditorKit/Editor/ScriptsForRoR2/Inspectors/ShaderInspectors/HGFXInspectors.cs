using System;
using UnityEditor;
using static RoR2EditorKit.Core.Inspectors.ExtendedMaterialInspector;
using BlendMode = UnityEngine.Rendering.BlendMode;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    public static class HGFXInspectors
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (MaterialEditorEnabled)
                AddShader("hgCloudRemap", HGCloudRemapEditor, typeof(HGFXInspectors));
        }

        public static void HGCloudRemapEditor()
        {
            DrawBlendEnumProperty(GetProperty("_SrcBlend"));
            DrawBlendEnumProperty(GetProperty("_DstBlend"));
            DrawProperty("_TintColor");
            DrawProperty("_DisableRemapOn");
            DrawProperty("_MainTex");
            DrawProperty("_RemapTex");
            DrawProperty("_InvFade");
            DrawProperty("_Boost");
            DrawProperty("_AlphaBoost");
            DrawProperty("_AlphaBias");
            DrawProperty("_UseUV1On");
            DrawProperty("_FadeCloseOn");
            DrawProperty("_FadeCloseDistance");
            DrawProperty("_Cull");
            DrawProperty("_ZTest");
            DrawProperty("_DepthOffset");
            DrawProperty("_CloudsOn");
            DrawProperty("_CloudOffsetOn");
            DrawProperty("_DistortionStrength");
            DrawProperty("_Cloud1Tex");
            DrawProperty("_Cloud2Tex");
            DrawProperty("_CutoffScroll");
            DrawProperty("_VertexColorOn");
            DrawProperty("_VertexAlphaOn");
            DrawProperty("_CalcTextureAlphaOn");
            DrawProperty("_VertexOffsetOn");
            DrawProperty("_FresnelOn");
            DrawProperty("_SkyboxOnly");
            DrawProperty("_FresnelPower");
            DrawProperty("_OffsetAmount");
        }

        private static void DrawBlendEnumProperty(MaterialProperty prop)
        {
            float value = prop.floatValue;
            prop.floatValue = Convert.ToSingle(EditorGUILayout.EnumPopup(prop.displayName, (BlendMode)prop.floatValue));
        }
    }
}