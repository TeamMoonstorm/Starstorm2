using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using System;
using R2API;
using RoR2.UI;

namespace SS2.Components
{
    public class StormController : MonoBehaviour
    {
        public static StormController instance;

        [SystemInitializer]
        //[RuntimeInitializeOnLoadMethod] // IDK WHY THIS DOESNT WORK. IT WORKS FOR PICKUPMAGNET. EWTF!!!!!!!!!!!!!!!!!!!
        static void Init()
        {
            Stage.onStageStartGlobal += (stage) =>
            {
                if(NetworkServer.active)
                {
                    GameObject stormController = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StormController", SS2Bundle.Events));
                    NetworkServer.Spawn(stormController);
                }
                
            };
        }
        [Serializable]
        public struct StormVFX
        {
            public R2API.DirectorAPI.Stage stageEnum;
            public string customStageName;
            public GameObject effectPrefab;
        }
        public StormVFX[] eventVFX = Array.Empty<StormVFX>();

        private void OnEnable()
        {
            if (StormController.instance)
            {
                Debug.LogError("Only one StormController can exist at a time.");
                return;
            }
            StormController.instance = this;
        }
        private void OnDisable()
        {
            if (StormController.instance == this)
            {
                StormController.instance = null;
            }
        }

        private void Start()
        {
            InstantiateEffect();
            this.SetIntensity(this.effectIntensity);
            ObjectivePanelController.collectObjectiveSources += StormObjective;           
        }
        private void OnDestroy()
        {
            ObjectivePanelController.collectObjectiveSources -= StormObjective;
        }

        private float endLerpIntensity;
        private float startLerpIntensity;
        private float lerpTimeScale;
        private float lerpTime = 1f;
        private float effectIntensity;

        public IIntensityScaler[] intensityScalers;

        private int stormLevel;
        private float levelPercentComplete;

        public void StartLerp(float newIntensity, float lerpDuration)
        {
            this.endLerpIntensity = newIntensity;
            if(lerpDuration > 0)
            {
                this.startLerpIntensity = effectIntensity;
                this.lerpTimeScale = 1 / lerpDuration;
                this.lerpTime = 0;
                return;
            }
            this.lerpTime = 1;
            this.lerpTimeScale = 0;
        }

        private void FixedUpdate()
        {
            this.lerpTime += Time.fixedDeltaTime * lerpTimeScale;
            if(this.lerpTime <= 1)
            {
                this.SetIntensity(Mathf.Lerp(startLerpIntensity, endLerpIntensity, this.lerpTime));
            }
            else
            {
                this.SetIntensity(endLerpIntensity);
            }
        }

        public void SetIntensity(float newIntensity)
        {
            if (newIntensity == this.effectIntensity) return;

            this.effectIntensity = newIntensity;

            foreach(IIntensityScaler scaler in this.intensityScalers)
            {
                scaler.SetIntensity(newIntensity);
            }
        }
        public void SetStormLevel(int stormLevel, float percentComplete)
        {
            this.stormLevel = stormLevel;
            this.levelPercentComplete = percentComplete;
        }

        public void InstantiateEffect()
        {
            GameObject effectPrefab = SS2Assets.LoadAsset<GameObject>("ThunderstormEffect", SS2Bundle.Events);
            foreach(StormVFX vfx in this.eventVFX)
            {
                if(DirectorAPI.GetStageEnumFromSceneDef(Stage.instance.sceneDef) == vfx.stageEnum)
                {
                    effectPrefab = vfx.effectPrefab;
                    break;
                }
            }
            this.intensityScalers = GameObject.Instantiate(effectPrefab, base.gameObject.transform).GetComponentsInChildren<IIntensityScaler>(true);
        }

        private void StormObjective(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> dest)
        {
            dest.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
            {
                master = master,
                objectiveType = typeof(StormObjectiveTracker),
                source = base.gameObject
            });
        }

        private class StormObjectiveTracker : ObjectivePanelController.ObjectiveTracker
        {
            public override string GenerateString()
            {
                StormController instance = StormController.instance;
                float intensity = (float)Math.Round(instance.effectIntensity, 1);
                return string.Format("effectIntensity: {0} hehe storm level {1}, {2}%", intensity, instance.stormLevel, Mathf.FloorToInt(instance.levelPercentComplete));
            }

            public override bool IsDirty()
            {
                return true;
            }
        }
    }
}

