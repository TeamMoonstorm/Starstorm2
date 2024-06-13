using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Linq;
namespace SS2.Components
{
    public class PostProcessingIntensity : MonoBehaviour, IIntensityScaler
    {
        [HideInInspector]
        public float currentIntensity = 0f;
        private PostProcessingState currentState;

        public PostProcessVolume postProcessVolume;
        private FloatParameter rainParam;
        public PostProcessingState[] breakpoints = Array.Empty<PostProcessingState>();

        private bool alive = true;
        private float minimumIntensity;
        private void Start()
        {
            if (!postProcessVolume) postProcessVolume = base.GetComponent<PostProcessVolume>();
            rainParam = postProcessVolume.profile.GetSetting<SobelRain>()?.rainIntensity;
            breakpoints = breakpoints.OrderBy(bp => bp.requiredIntensity).ToArray();

            if (minimumIntensity > currentIntensity)
            {
                alive = false;
                base.gameObject.SetActive(false);
            }
        }

        public void SetIntensity(float intensity)
        {
            if (intensity == currentIntensity || breakpoints.Length == 0) return;
            this.currentIntensity = intensity;

            bool alive = intensity > minimumIntensity;
            if (this.alive != alive)
            {
                this.alive = alive;
                base.gameObject.SetActive(alive);
            }

            PostProcessingState lower = breakpoints[0];
            PostProcessingState upper = breakpoints[breakpoints.Length - 1];
            for (int i = breakpoints.Length - 1; i >= 0; i--)
            {
                if (intensity > breakpoints[i].requiredIntensity)
                {
                    lower = breakpoints[i];
                    if (i + 1 >= breakpoints.Length) upper = lower;
                    else upper = breakpoints[i + 1];

                    break;
                }
            }
            float t = intensity - lower.requiredIntensity;
            float tMax = upper.requiredIntensity - lower.requiredIntensity;
            PostProcessingState state = PostProcessingState.Lerp(ref lower, ref upper, t / tMax);

            if (!currentState.Equals(state))
            {
                currentState = state;

                postProcessVolume.weight = state.weight;
                if(this.rainParam != null)
                    rainParam.value = state.rainIntensity;
            }
        }

        [Serializable]
        public struct PostProcessingState
        {
            public float requiredIntensity;

            public float weight;
            public float rainIntensity;

            public static PostProcessingState Lerp(ref PostProcessingState a, ref PostProcessingState b, float t)
            {
                return new PostProcessingState
                {
                    weight = Mathf.LerpUnclamped(a.weight, b.weight, t),
                    rainIntensity = Mathf.LerpUnclamped(a.rainIntensity, b.rainIntensity, t),
                };
            }

            public bool Equals(PostProcessingState other)
            {
                return requiredIntensity == other.requiredIntensity
                    && weight == other.weight
                    && rainIntensity == other.rainIntensity;
            }
        }
    }
}