using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


namespace Moonstorm.Starstorm2.PostProcess
{
    [PostProcess(typeof(SS2SobelRainRenderer), PostProcessEvent.BeforeTransparent, "SS2/SobelRain", true)]
    [Serializable]
    public sealed class SS2SobelRain : PostProcessEffectSettings
    {
        [Range(0f, 100f)]
        [Tooltip("The intensity of the rain.")]
        public FloatParameter rainIntensity = new FloatParameter
        {
            value = 0.5f
        };

        [Range(0f, 10f)]
        [Tooltip("The falloff of the outline. Higher values means it relies less on the sobel.")]
        public FloatParameter outlineScale = new FloatParameter
        {
            value = 1f
        };

        [Tooltip("The density of rain.")]
        [Range(0f, 1f)]
        public FloatParameter rainDensity = new FloatParameter
        {
            value = 1f
        };

        public TextureParameter rainTexture = new TextureParameter
        {
            value = null
        };

        public ColorParameter rainColor = new ColorParameter
        {
            value = Color.white
        };
    }
}