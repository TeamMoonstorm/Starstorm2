using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Moonstorm.Starstorm2.PostProcess
{
    public sealed class SS2SobelRainRenderer : PostProcessEffectRenderer<SS2SobelRain>
    {
        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("Hidden/PostProcess/SobelRain"));
            if(propertySheet != null)
            {
                propertySheet.properties.SetFloat("_RainIntensity", settings.rainIntensity);
                propertySheet.properties.SetFloat("_OutlineScale", settings.outlineScale);
                propertySheet.properties.SetFloat("_RainDensity", settings.rainDensity);
                propertySheet.properties.SetTexture("_RainTexture", settings.rainTexture);
                propertySheet.properties.SetColor("_RainColor", settings.rainColor);
                context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0, false, null);
            }
            else
            {
                SS2Log.Info("SS2RampFogRenderer property sheet was null");
            }
        }
    }
}