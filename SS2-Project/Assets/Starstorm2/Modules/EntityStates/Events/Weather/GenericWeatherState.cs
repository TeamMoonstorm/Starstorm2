using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Buffs;
using Moonstorm.Starstorm2.Components;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Events
{
    public abstract class GenericWeatherState : EventState
    {
        [SerializeField]
        public GameObject effectPrefab;
        [SerializeField]
        public static float fadeDuration = 7f;


        private GameObject effectInstance;
        private EventStateEffect eventStateEffect;
        public override void OnEnter()
        {
            base.OnEnter();
            if (effectPrefab)
            {
                effectInstance = Object.Instantiate(effectPrefab);
                eventStateEffect = effectInstance.GetComponent<EventStateEffect>();
                if (eventStateEffect)
                {
                    eventStateEffect.intensityMultiplier = DiffScalingValue;
                }
            }
        }

        public override void StartEvent()
        {
            base.StartEvent();
            if (eventStateEffect)
            {
                eventStateEffect.OnEffectStart();
            }

            RoR2.Run.onRunDestroyGlobal += RunEndRemoveWeather;
            RoR2.Stage.onServerStageComplete += StageEndRemoveWeather;
        }

        private void StageEndRemoveWeather(Stage obj)
        {
            RoR2.Run.onRunDestroyGlobal -= RunEndRemoveWeather;
            RoR2.Stage.onServerStageComplete -= StageEndRemoveWeather;
        }

        private void RunEndRemoveWeather(Run obj)
        {
            RoR2.Run.onRunDestroyGlobal -= RunEndRemoveWeather;
            RoR2.Stage.onServerStageComplete -= StageEndRemoveWeather;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (eventStateEffect)
            {
                eventStateEffect.OnEndingStart(fadeDuration);
            }
            if (HasWarned)
            {
                RoR2.Run.onRunDestroyGlobal -= RunEndRemoveWeather;
                RoR2.Stage.onServerStageComplete -= StageEndRemoveWeather;
            }
        }
    }

}