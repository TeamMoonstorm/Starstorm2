using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using RoR2;
namespace SS2
{
    public class FlashingEffectIntensity : MonoBehaviour
    {
        public Light[] lights = new Light[0];
        public PostProcessVolume[] pps = new PostProcessVolume[0];


        private float intensity;
        private void Awake()
        {
            intensity = Mathf.Clamp01(SS2Config.FlashingEffectsIntensity);

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].TryGetComponent(out LightIntensityCurve curve))
                {
                    curve.maxIntensity = lights[i].intensity * intensity;
                }    
                lights[i].intensity *= intensity;
            }

            for (int i = 0; i < pps.Length; i++)
            {
                if (pps[i].TryGetComponent(out PostProcessDuration ppDuration))
                {
                    for (int k = 0; k < ppDuration.ppWeightCurve.keys.Length; k++)
                    {
                        ppDuration.ppWeightCurve.keys[k].value *= intensity;
                    }
                }
                pps[i].weight *= intensity;
            }
        }
    }
}
