using System;
using ThreeEyedGames;
using UnityEngine;
using System.Linq;

namespace SS2.Components
{
    public class LightCausticsIntensity : MonoBehaviour, IIntensityScaler
    {
        private float currentIntensity;

        public Texture cookie;
        private Material material;

        public float minimumIntensity;
        public CausticsState[] breakpoints = Array.Empty<CausticsState>();

        private bool alive = true;
        private CausticsState currentState;

        private void Awake()
        {          
            NGSS_Directional sun = GameObject.FindObjectOfType<NGSS_Directional>();
            if(!sun)
            {
                SS2Log.Warning("LightCausticsIntensity.Start(): No sun found.");
                Destroy(this);
                return;
            }

            breakpoints = breakpoints.OrderBy(bp => bp.requiredIntensity).ToArray();
            Light light = sun.GetComponent<Light>();
            light.cookie = cookie;
            light.cookieSize = 150;
            CustomRenderTexture customRenderTexture = (CustomRenderTexture)light.cookie;
            if(customRenderTexture)
            {
                material = customRenderTexture.material;
                if(!material)
                {
                    SS2Log.Warning("LightCausticsIntensity.Start(): No light material found. (????)");
                    Destroy(this);
                    return;
                }
                material.SetFloat("_AlphaBias", 1);
            }

            if (minimumIntensity >= currentIntensity)
            {
                alive = false;
                base.gameObject.SetActive(false);
            }
        }
        public void SetIntensity(float intensity)
        {
            currentIntensity = intensity;

            if (breakpoints.Length == 0) return;


            bool alive = intensity >= minimumIntensity;
            if (this.alive != alive)
            {
                this.alive = alive;
                base.gameObject.SetActive(alive); ;
            }

            CausticsState lower = breakpoints[0];
            CausticsState upper = breakpoints[breakpoints.Length - 1];
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
            CausticsState state = CausticsState.Lerp(ref lower, ref upper, t / tMax);

            if (!currentState.Equals(state))
            {
                if(material) // ??????????????? bob omb battlefield deletes it
                    material.SetFloat("_AlphaBias", state.alphaBias);

                this.currentState = state;
            }
        }

        [Serializable]
        public struct CausticsState : IEquatable<CausticsState>
        {
            public float requiredIntensity;

            public float alphaBias;
            public static CausticsState Lerp(ref CausticsState a, ref CausticsState b, float t)
            {
                if (a.Equals(b)) return a;
                return new CausticsState
                {
                    alphaBias = Mathf.LerpUnclamped(a.alphaBias, b.alphaBias, t),
                };
            }

            public bool Equals(CausticsState other)
            {
                return requiredIntensity == other.requiredIntensity
                    && alphaBias == other.alphaBias;
            }
        }
    }
}
