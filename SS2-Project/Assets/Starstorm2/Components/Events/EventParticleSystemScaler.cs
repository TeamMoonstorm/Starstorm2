using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(ParticleSystem))]
    public class EventParticleSystemScaler : MonoBehaviour
    {
        [Tooltip("Based off the difficulty scaling value of the DifficultyDef.")]
        public float particlesPerDifficultyIncrease = 0f;

        [Tooltip("How much the speed of the particle increases per difficulty increase.")]
        public float particleSpeedPerDifficultyIncrease = 0f;

        [Tooltip("How much the speed of the particle increases per difficulty increase.")]
        public float simulationSpeedPerDifficultyIncrease = 0f;

        //Typhoon cap
        public static float difficultyScaleCap = 3.5f;

        private ParticleSystem particleSystem;
        private ParticleSystem.MainModule particleMain;

        private void Awake()
        {
            if (Run.instance)
            {
                particleSystem = GetComponent<ParticleSystem>();
                particleMain = particleSystem.main;
                float scaling = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue;
                float increaseCoefficient = Mathf.Clamp(scaling - 1f, 0, difficultyScaleCap - 1f);
                if (particleSpeedPerDifficultyIncrease != 0)
                    particleMain.startSpeedMultiplier += particleSpeedPerDifficultyIncrease * increaseCoefficient;
                if (particlesPerDifficultyIncrease != 0)
                {
                    var emission = particleSystem.emission;
                    emission.rateOverTimeMultiplier += particlesPerDifficultyIncrease * increaseCoefficient;
                }
                if (simulationSpeedPerDifficultyIncrease != 0)
                    particleMain.simulationSpeed += simulationSpeedPerDifficultyIncrease * increaseCoefficient;
            }
            Destroy(this);
        }
    }
}