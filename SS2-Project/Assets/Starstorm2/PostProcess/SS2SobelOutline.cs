using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


namespace Moonstorm.Starstorm2.PostProcess
{
    [PostProcess(typeof(SS2SobelOutlineRenderer), PostProcessEvent.BeforeTransparent, "SS2/SobelOutline", true)]
    [Serializable]
    public sealed class SS2SobelOutline : PostProcessEffectSettings
    {
        // Token: 0x04000108 RID: 264
        [Tooltip("The intensity of the outline.")]
        [Range(0f, 5f)]
        public FloatParameter outlineIntensity = new FloatParameter
        {
            value = 0.5f
        };

        // Token: 0x04000109 RID: 265
        [Range(0f, 10f)]
        [Tooltip("The falloff of the outline.")]
        public FloatParameter outlineScale = new FloatParameter
        {
            value = 1f
        };
    }
}