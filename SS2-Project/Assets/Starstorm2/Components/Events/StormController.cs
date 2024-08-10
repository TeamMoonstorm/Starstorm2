using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using System;
using R2API;
using RoR2.UI;
using EntityStates;
using EntityStates.Events;
using UnityEngine.Events;
using static MSU.GameplayEventTextController;
namespace SS2.Components
{
    public class StormController : NetworkBehaviour
    {
        public static StormController instance;

        public static readonly Xoroshiro128Plus chargeRng = new Xoroshiro128Plus(0UL);
        public static readonly Xoroshiro128Plus mobChargeRng = new Xoroshiro128Plus(0UL);
        public static readonly Xoroshiro128Plus treasureRng = new Xoroshiro128Plus(0UL);
        public static PickupDropTable dropTable;
        public static bool shouldShowObjective = false; // weather radio? config setting?
        private static Color etherealTextColor = new Color(.204f, .921f, .561f);

        [SystemInitializer]
        //[RuntimeInitializeOnLoadMethod] // IDK WHY THIS DOESNT WORK
        static void Init()
        {
            // custom drop table? maybe?
            // souls soon.............

            StormController.dropTable = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/Chest1/dtChest1.asset").WaitForCompletion();
            Stage.onServerStageBegin += OnServerStageBegin;
        }

        private static void OnServerStageBegin(Stage stage)
        {
            if(stage.sceneDef.sceneType == SceneType.Stage && TeleporterInteraction.instance) // this should cover every stage we want storms on? im pretty sure
            {
                chargeRng.ResetSeed(Run.instance.treasureRng.nextUlong);
                mobChargeRng.ResetSeed(Run.instance.treasureRng.nextUlong);
                treasureRng.ResetSeed(Run.instance.treasureRng.nextUlong);

                GameObject stormController = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StormController", SS2Bundle.Events));
                NetworkServer.Spawn(stormController);
            }
   
        }


        // START STORM 2!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [ConCommand(commandName = "start_storm", flags = ConVarFlags.Cheat | ConVarFlags.ExecuteOnServer, helpText = "Sets the current storm level. Zero to disable. Format: {stormLevel}")]
        public static void CCSetStormLevel(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            int level = args.GetArgInt(0);
            StormController stormController = StormController.instance;
            if(!stormController)
            {
                stormController = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StormController", SS2Bundle.Events)).GetComponent<StormController>();
                NetworkServer.Spawn(stormController.gameObject);              
            }

            stormController.ForceStormLevel(level);

        }
        [Serializable]
        public struct StormVFX
        {
            public R2API.DirectorAPI.Stage stageEnum;
            public string customStageName;
            public GameObject effectPrefab;
        }
        public StormVFX[] eventVFX = Array.Empty<StormVFX>();

        private float endLerpIntensity;
        private float startLerpIntensity;
        private float lerpTimeScale;
        private float lerpTime = 1f;
        private float effectIntensity;

        public IIntensityScaler[] intensityScalers;

        private int stormLevel;
        public int MaxStormLevel { get; private set; }
        public bool IsPermanent { get; private set; }

        private float levelPercentComplete;
        private Run.FixedTimeStamp stormStartTime;

        private EntityStateMachine stateMachine;

        [SyncVar]
        private float chargeInCurrentLevel;
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
            this.stateMachine = base.GetComponent<EntityStateMachine>();
            this.SetEffectIntensity(this.effectIntensity);

#if DEBUG
            shouldShowObjective = true;
#endif

            stormStartTime = Run.FixedTimeStamp.now + UnityEngine.Random.Range(180, 360); // TODO: Event director that handles this. this fucking sux lol

            DifficultyDef difficulty = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
            MaxStormLevel = Mathf.FloorToInt(difficulty.scalingValue) + 1;
            MaxStormLevel = Mathf.Clamp(MaxStormLevel, 2, 4); // drizzle 2, monsoon/typhoon+ 4

            IsPermanent = difficulty.scalingValue >= Typhoon.sdd.scalingValue; // TODO: RunFlags. difficulties r a shit
            if (shouldShowObjective)
                ObjectivePanelController.collectObjectiveSources += StormObjective;
         
        }
             
        private EntityState InstantiateInitialState()
        {

            return new Calm();
        }
        // TODO: Stage cooldown on skipping the initial "calm" level
        public bool AttemptSkip(int consecutiveSkips = 0)
        {
            float chance = 0;
            int etherealsCompleted = EtherealBehavior.instance.etherealsCompleted;
            if (etherealsCompleted > 0)
                chance = (25f + 10f * (etherealsCompleted - 1)) * Mathf.Pow(0.5f, consecutiveSkips);
            bool doSkip = Util.CheckRoll(chance);
            return Util.CheckRoll(chance);
        }

        private void OnDestroy()
        {
            ObjectivePanelController.collectObjectiveSources -= StormObjective;
        }

        
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
            this.levelPercentComplete = Mathf.Clamp(this.chargeInCurrentLevel, 0, 100);

            this.lerpTime += Time.fixedDeltaTime * lerpTimeScale;
            if(this.lerpTime <= 1)
            {
                this.SetEffectIntensity(Mathf.Lerp(startLerpIntensity, endLerpIntensity, this.lerpTime));
            }
            else
            {
                this.SetEffectIntensity(endLerpIntensity);
            }
        }

        public void SetEffectIntensity(float newIntensity)
        {
            if (newIntensity == this.effectIntensity) return;

            this.effectIntensity = newIntensity;

            foreach(IIntensityScaler scaler in this.intensityScalers)
            {
                scaler.SetIntensity(newIntensity);
            }
        }

        [Server]
        public void ForceStormLevel(int stormLevel)
        {
            if(stormLevel > 0)
            {
                this.stateMachine.SetNextState(new EntityStates.Events.Storm { stormLevel = stormLevel, lerpDuration = 5f });
            }
            else
            {
                this.stateMachine.SetNextState(new Calm());
            }

            this.stormLevel = stormLevel;
        }

        public void OnStormLevelCompleted()
        {
            this.stormLevel++;
        }

        [Server]
        public void AddCharge(float charge)
        {
            this.chargeInCurrentLevel += charge;         
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

        public class EtherealFadeIn : EventTextState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                Juice.destroyOnEndOfTransition = false;
                Juice.transitionDuration = duration;
                Juice.TransitionAlphaFadeIn();
                Juice.originalAlpha = 1;
                Juice.transitionEndAlpha = 1;
            }

            public override void Update()
            {
                base.Update();
                if (age > duration)
                {
                    outer.SetNextState(new EtherealBlinkOut
                    {
                        duration = duration
                    });
                }
            }
        }

        public class EtherealBlinkOut : EventTextState
        {
            public static float endX = 0f;
            public static float endY = 4f;
            public static float FUCK = 2350;
            private static float blinkDuration = 0.07f;
            public override void OnEnter()
            {
                base.OnEnter();
                duration = .5f;
            }

            public override void Update()
            {
                base.Update();
                float t = Mathf.Clamp01(age / blinkDuration);
                float x = Mathf.Lerp(1, endX, t);
                float y = Mathf.Lerp(1, endY, t);
                float fuck = Mathf.Lerp(690, FUCK, t);
                base.transform.localScale = new Vector3(x, y, 1);              
                base.transform.localPosition = new Vector3(0, fuck, 0); // the text isnt fucking centered so scaling it is fucked
                if (age > duration)
                {
                    outer.SetNextState(new EtherealBlinkIn
                    {
                        duration = duration
                    });
                }
            }
        }

        public class EtherealBlinkIn : EventTextState
        {
            private static float blinkDuration = 0.07f;
            public override void OnEnter()
            {
                base.OnEnter();
                // ??????????????????? i hate this so much
                Juice.transitionDuration = 0.01f;
                Juice.TransitionAlphaFadeIn();
                Juice.originalAlpha = 1;
                Juice.transitionEndAlpha = 1;

                base.TextController.TextMeshProUGUI.color = etherealTextColor;
                base.TextController.TextMeshProUGUI.SetText("<link=\"textShaky\">ethereal moment</link>");// String.Format("<link=\"textShaky\">{0}</link>", TextController.TextMeshProUGUI.text));
                EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("EtherealStormWarning", SS2Bundle.Events), new EffectData { }, false);
                duration = blinkDuration;
            }

            public override void Update()
            {
                base.Update();
                float t = Mathf.Clamp01(age / duration);
                float x = Mathf.Lerp(EtherealBlinkOut.endX, 1, t);
                float y = Mathf.Lerp(EtherealBlinkOut.endY, 1, t);
                float fuck = Mathf.Lerp(EtherealBlinkOut.FUCK, 690, t);
                base.transform.localScale = new Vector3(x, y, 1);
                base.transform.localPosition = new Vector3(0, fuck, 0); // the text isnt fucking centered so scaling it is fucked

                if (age > duration)
                {                 
                    outer.SetNextState(new WaitState
                    {
                        duration = 3f
                    });
                }
            }
        }
        public class EtherealWait : EventTextState
        {
            public override void Update()
            {
                if (age > duration)
                {
                    outer.SetNextState(new EtherealFadeOut
                    {
                        duration = 2f
                    });
                }
            }
        }
        public class EtherealFadeOut : EventTextState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                Juice.transitionDuration = duration;
                Juice.TransitionAlphaFadeOut();
            }

            public override void Update()
            {
                base.Update();
                if (age > duration)
                {
                    NullRequest();
                    
                    outer.SetNextStateToMain();
                }
            }
        }
    }
}

