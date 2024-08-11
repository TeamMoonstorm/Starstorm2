using System;
using UnityEngine;
using System.Linq;
namespace SS2.Components
{
    public class ParticleSystemIntensity : MonoBehaviour, IIntensityScaler
    {
        private float currentIntensity;
        
        public ParticleSystem particleSystem;
        
        public float minimumIntensity;
        public ParticleSystemState[] breakpoints = Array.Empty<ParticleSystemState>();

        private bool alive = true;
        private ParticleSystemState currentState;
        private ParticleSystem.EmissionModule em;
        private ParticleSystem.MainModule main;
        private void Start()
        {
            if (!particleSystem) particleSystem = base.GetComponent<ParticleSystem>();
            em = particleSystem.emission;
            main = particleSystem.main;
            breakpoints = breakpoints.OrderBy(bp => bp.requiredIntensity).ToArray();

            if(minimumIntensity >= currentIntensity)
            {
                alive = false;
                base.gameObject.SetActive(false);
            }
        }
        public void SetIntensity(float intensity)
        {
            currentIntensity = intensity;

            if (breakpoints.Length == 0) return;

            main = particleSystem.main; // this shit makes no sense man wtf
            bool alive = intensity >= minimumIntensity;
            if(this.alive != alive)
            {
                this.alive = alive;               
                base.gameObject.SetActive(alive);
                if(!main.playOnAwake)
                    particleSystem.Play();
            }

            ParticleSystemState lower = breakpoints[0];
            ParticleSystemState upper = breakpoints[breakpoints.Length - 1];
            for(int i = breakpoints.Length - 1; i >= 0; i--)
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
            ParticleSystemState state = ParticleSystemState.Lerp(ref lower, ref upper, t / tMax);

            if(!currentState.Equals(state))
            {

                if(state.alpha != -1)
                {                
                    Color color = main.startColor.color;
                    color.a = state.alpha;
                    main.startColor = new ParticleSystem.MinMaxGradient(color); // we love unity
                }
                if (state.startSize != -1) main.startSize = state.startSize;
                if (state.startSpeed != -1) main.startSpeedMultiplier = state.startSpeed;
                if (state.simulationSpeed != -1) main.simulationSpeed = state.simulationSpeed;
                if (state.rateOverTime != -1) em.rateOverTimeMultiplier = state.rateOverTime;
                //want to rotate around 0,0 instead of 0,300
                // im stupid sry
                float deltaX = state.rotation.x - currentState.rotation.x;
                float deltaY = state.rotation.y - currentState.rotation.y;
                float deltaZ = state.rotation.z - currentState.rotation.z;
                transform.RotateAround(Vector3.zero, Vector3.forward, deltaX);
                transform.RotateAround(Vector3.zero, Vector3.up, deltaY);
                transform.RotateAround(Vector3.zero, Vector3.right, deltaZ);

                this.currentState = state;
            }    
        }

        [Serializable]
        public struct ParticleSystemState : IEquatable<ParticleSystemState>
        {
            public float requiredIntensity;

            public float alpha;
            public float startSpeed;
            public float startSize;
            public float rateOverTime;
            public float simulationSpeed;
            public Vector3 rotation;

            public static ParticleSystemState Lerp(ref ParticleSystemState a, ref ParticleSystemState b, float t)
            {
                if (a.Equals(b)) return a;
                return new ParticleSystemState
                {
                    alpha = Mathf.LerpUnclamped(a.alpha, b.alpha, t),
                    startSpeed = Mathf.LerpUnclamped(a.startSpeed, b.startSpeed, t),
                    startSize = Mathf.LerpUnclamped(a.startSize, b.startSize, t),
                    rateOverTime = Mathf.LerpUnclamped(a.rateOverTime, b.rateOverTime, t),
                    simulationSpeed = Mathf.LerpUnclamped(a.simulationSpeed, b.simulationSpeed, t),
                    rotation = Vector3.Lerp(a.rotation, b.rotation, t),
                };
            }

            public bool Equals(ParticleSystemState other)
            {
                return requiredIntensity == other.requiredIntensity
                    && alpha == other.alpha
                    && startSpeed == other.startSpeed
                    && startSize == other.startSize
                    && rateOverTime == other.rateOverTime
                    && simulationSpeed == other.simulationSpeed
                    && rotation == other.rotation;
            }
        }
    }
}