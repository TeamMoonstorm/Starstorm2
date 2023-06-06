using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Moonstorm.Starstorm2.PostProcess
{
    public sealed class SS2RampFogRenderer : PostProcessEffectRenderer<SS2RampFog>
    {
        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("Hidden/PostProcess/RampFog"));
            if (propertySheet != null)
            {
                propertySheet.properties.SetFloat("_FogIntensity", settings.fogIntensity);
                propertySheet.properties.SetFloat("_FogPower", settings.fogPower);
                propertySheet.properties.SetFloat("_FogZero", settings.fogZero);
                propertySheet.properties.SetFloat("_FogOne", settings.fogOne);
                propertySheet.properties.SetFloat("_FogHeightStart", settings.fogHeightStart);
                propertySheet.properties.SetFloat("_FogHeightEnd", settings.fogHeightEnd);
                propertySheet.properties.SetFloat("_FogHeightIntensity", settings.fogHeightIntensity);
                propertySheet.properties.SetColor("_FogColorStart", settings.fogColorStart);
                propertySheet.properties.SetColor("_FogColorMid", settings.fogColorMid);
                propertySheet.properties.SetColor("_FogColorEnd", settings.fogColorEnd);
                propertySheet.properties.SetFloat("_SkyboxStrength", settings.skyboxStrength);
                context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0, false, null);
            }
            else
            {
                SS2Log.Info("SS2RampFogRenderer property sheet was null");
            }

        }
    }
}