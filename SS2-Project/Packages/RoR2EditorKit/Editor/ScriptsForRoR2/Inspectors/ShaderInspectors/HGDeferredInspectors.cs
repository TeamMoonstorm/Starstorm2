using UnityEditor;
using static RoR2EditorKit.Core.Inspectors.ExtendedMaterialInspector;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    /// <summary>
    /// RoR2EK's HopooGames/Deferred shader editors.
    /// </summary>
    public static class HGDeferredInspectors
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (MaterialEditorEnabled)
            {
                AddShader("hgStandard", HGStandardEditor, typeof(HGDeferredInspectors));
                AddShader("hgSnowtopped", HGSnowtoppedEditor, typeof(HGDeferredInspectors));
            }
        }

        private static void HGStandardEditor()
        {
            DrawProperty("_EnableCutout");
            DrawProperty("_Color");
            DrawProperty("_MainTex");
            DrawProperty("_NormalStrength");
            DrawProperty("_NormalTex");
            DrawProperty("_EmColor");
            DrawProperty("_EmTex");
            DrawProperty("_EmPower");
            DrawProperty("_Smoothness");
            DrawProperty("_ForceSpecOn");
            DrawProperty("_RampInfo");
            DrawProperty("_DecalLayer");
            DrawProperty("_SpecularStrength");
            DrawProperty("_SpecularExponent");
            DrawProperty("_Cull");

            var prop = DrawProperty("_DitherOn");
            if (ShaderKeyword(prop))
            {
                DrawProperty("_FadeBias");
            }

            prop = DrawProperty("_FEON");
            if (ShaderKeyword(prop))
            {
                DrawProperty("_FresnelRamp");
                DrawProperty("_FresnelPower");
                DrawProperty("_FresnelMask");
                DrawProperty("_FresnelBoost");
            }

            prop = DrawProperty("_PrintOn");
            if (ShaderKeyword(prop))
            {
                DrawProperty("_SliceHeight");
                DrawProperty("_SliceBandHeight");
                DrawProperty("_SliceAlphaDepth");
                DrawProperty("_SliceAlphaTex");
                DrawProperty("_PrintBoost");
                DrawProperty("_PrintEmissionToAlbedoLerp");
                DrawProperty("_PrintDirection");
                DrawProperty("_PrintRamp");
            }

            Header("Elite Ramp");
            DrawProperty("_EliteBrightnessMin");
            DrawProperty("_EliteBrightnessMax");

            prop = DrawProperty("_SplatmapOn");
            if (ShaderKeyword(prop))
            {
                DrawProperty("_ColorsOn");
                DrawProperty("_Depth");
                DrawProperty("_SplatmapTex");
                DrawProperty("_SplatmapTileScale");
                DrawProperty("_GreenChannelTex");
                DrawProperty("_GreenChannelNormalTex");
                DrawProperty("_GreenChannelSmoothness");
                DrawProperty("_GreenChannelBias");
                DrawProperty("_BlueChannelTex");
                DrawProperty("_BlueChannelNormalTex");
                DrawProperty("_BlueChannelSmoothness");
                DrawProperty("_BlueChannelBias");
            }

            prop = DrawProperty("_FlowmapOn");
            if (ShaderKeyword(prop))
            {
                DrawProperty("_FlowTex");
                DrawProperty("_FlowHeightmap");
                DrawProperty("_FlowHeightRamp");
                DrawProperty("_FlowHeightBias");
                DrawProperty("_FlowHeightPower");
                DrawProperty("_FlowEmissionStrength");
                DrawProperty("_FlowSpeed");
                DrawProperty("_FlowMaskStrength");
                DrawProperty("_FlowNormalStrength");
                DrawProperty("_FlowTextureScaleFactor");
            }

            DrawProperty("_LimbRemovalOn");
        }

        private static void HGSnowtoppedEditor()
        {
            DrawProperty("_Color");
            DrawProperty("_MainTex");
            DrawProperty("_SnowTex");
            DrawProperty("_SnowNormalTex");
            DrawProperty("_SnowBias");
            DrawProperty("_Depth");
            DrawProperty("_IgnoreBiasOn");
            DrawProperty("_BinaryBlendOn");
            DrawProperty("_RampInfo");
            DrawProperty("_ForceSpecOn");
            DrawProperty("_SpecularStrength");
            DrawProperty("_SpecularExponent");
            DrawProperty("_SnowSmoothness");
            DrawProperty("_DitherOn");
            var prop = DrawProperty("_TriplanarOn");
            if (ShaderKeyword(prop))
            {
                DrawProperty("_TriplanarTextureFactor");
                DrawProperty("_SnowOn");
            }
            prop = DrawProperty("_GradientBiasOn");
            if (ShaderKeyword(prop))
            {
                DrawProperty("_GradientBiasVector");
            }
            prop = DrawProperty("_DirtOn");
            if (ShaderKeyword(prop))
            {
                DrawProperty("_DirtTex");
                DrawProperty("_DirtNormalTex");
                DrawProperty("_DirtBias");
                DrawProperty("_DirtSpecularStrength");
                DrawProperty("_DirtSpecularExponent");
                DrawProperty("_DirtSmoothness");
            }
        }
    }
}