using System;
using ThreeEyedGames;
using UnityEngine;
using System.Linq;

namespace SS2.Components
{
	public class ShaderAlphaIntensity : MonoBehaviour, IIntensityScaler
	{
        private float currentIntensity;

        public Renderer targetRenderer;

        public float minimumIntensity;
        public ShaderAlphaState[] breakpoints = Array.Empty<ShaderAlphaState>();

        private bool alive = true;
        private ShaderAlphaState currentState;
        private Material[] materials;
        private MaterialPropertyBlock _propBlock;
        private void Start()
        {
            this.targetRenderer = base.GetComponent<Renderer>();
            if (this.targetRenderer)
            {
                this.materials = this.targetRenderer.materials;
            }
            breakpoints = breakpoints.OrderBy(bp => bp.requiredIntensity).ToArray();

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
                base.gameObject.SetActive(alive);;
            }

            ShaderAlphaState lower = breakpoints[0];
            ShaderAlphaState upper = breakpoints[breakpoints.Length - 1];
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
            ShaderAlphaState state = ShaderAlphaState.Lerp(ref lower, ref upper, t / tMax);

            if (!currentState.Equals(state))
            {
                foreach (Material material in this.materials) // idk why this is iterating but vanilla does this and im scared
                {
                    this._propBlock = new MaterialPropertyBlock();
                    this.targetRenderer.GetPropertyBlock(this._propBlock);
                    this._propBlock.SetFloat("_ExternalAlpha", state.alpha);
                    this.targetRenderer.SetPropertyBlock(this._propBlock);
                }
                this.currentState = state;
            }
        }

        [Serializable]
        public struct ShaderAlphaState : IEquatable<ShaderAlphaState>
        {
            public float requiredIntensity;

            public float alpha;
            public static ShaderAlphaState Lerp(ref ShaderAlphaState a, ref ShaderAlphaState b, float t)
            {
                if (a.Equals(b)) return a;
                return new ShaderAlphaState
                {
                    alpha = Mathf.LerpUnclamped(a.alpha, b.alpha, t),
                };
            }

            public bool Equals(ShaderAlphaState other)
            {
                return requiredIntensity == other.requiredIntensity
                    && alpha == other.alpha;
            }
        }
    }
}
