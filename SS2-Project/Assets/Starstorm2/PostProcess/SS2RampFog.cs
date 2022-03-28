using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Moonstorm.Starstorm2.PostProcess
{
    [PostProcess(typeof(SS2RampFogRenderer), PostProcessEvent.BeforeTransparent, "SS2/RampFog", true)]
    [Serializable]
    public sealed class SS2RampFog : PostProcessEffectSettings
    {
        [Range(0f, 1f)]
        [Tooltip("Fog intensity.")]
        public FloatParameter fogIntensity = new FloatParameter
        {
            value = 0.5f
        };

        [Range(0f, 20f)]
        [Tooltip("Fog Power")]
        public FloatParameter fogPower = new FloatParameter
        {
            value = 1f
        };

        [Range(-1f, 1f)]
        [Tooltip("The zero value for the fog depth remap.")]
        public FloatParameter fogZero = new FloatParameter
        {
            value = 0f
        };

        [Tooltip("The one value for the fog depth remap.")]
        [Range(-1f, 1f)]
        public FloatParameter fogOne = new FloatParameter
        {
            value = 1f
        };

        [Tooltip("The world position value where the height fog begins.")]
        [Range(-100f, 100f)]
        public FloatParameter fogHeightStart = new FloatParameter
        {
            value = 0f
        };

        [Tooltip("The world position value where the height fog ends.")]
        [Range(-100f, 600f)]
        public FloatParameter fogHeightEnd = new FloatParameter
        {
            value = 100f
        };

        [Range(0f, 5f)]
        [Tooltip("The overall strength of the height fog.")]
        public FloatParameter fogHeightIntensity = new FloatParameter
        {
            value = 0f
        };

        [Tooltip("Color of the fog at the beginning.")]
        public ColorParameter fogColorStart = new ColorParameter
        {
            value = Color.white
        };

        [Tooltip("Color of the fog at the middle.")]
        public ColorParameter fogColorMid = new ColorParameter
        {
            value = Color.white
        };

        [Tooltip("Color of the fog at the end.")]
        public ColorParameter fogColorEnd = new ColorParameter
        {
            value = Color.white
        };

        [Tooltip("How much of the skybox will leak through?")]
        [Range(0f, 1f)]
        public FloatParameter skyboxStrength = new FloatParameter
        {
            value = 0f
        };
    }
}
