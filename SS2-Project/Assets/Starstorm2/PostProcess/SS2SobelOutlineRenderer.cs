using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Moonstorm.Starstorm2.PostProcess
{
    public sealed class SS2SobelOutlineRenderer : PostProcessEffectRenderer<SS2SobelOutline>
    {
        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("Hidden/PostProcess/SobelOutline"));
            if(propertySheet != null)
            {
                propertySheet.properties.SetFloat("_OutlineIntensity", settings.outlineIntensity);
                propertySheet.properties.SetFloat("_OutlineScale", settings.outlineScale);
                context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0, false, null);
            }
            else
            {
                SS2Log.Info("SS2RampFogRenderer property sheet was null");
            }
        }
    }
}