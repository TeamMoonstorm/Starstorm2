using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Moonstorm.Starstorm2.Components
{
    public class LerpRainIntensity : MonoBehaviour
    {
        public PostProcessVolume postProcessVolume;
        public float endIntensity;
        public float duration;

        private FloatParameter rainIntensity;
        private float stopwatch;
        private float startIntensity;
        private void Awake()
        {
            rainIntensity = postProcessVolume.profile.GetSetting<SobelRain>().rainIntensity;
        }

        private void Start()
        {
            startIntensity = rainIntensity;
        }

        void FixedUpdate()
        {
            rainIntensity.Interp(startIntensity, endIntensity, Mathf.Clamp01(stopwatch / duration));
            stopwatch += Time.fixedDeltaTime;

            if (stopwatch > duration)
                enabled = false;
        }
    }
}