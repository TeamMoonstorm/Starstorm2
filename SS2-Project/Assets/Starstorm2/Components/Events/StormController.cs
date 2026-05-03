using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using System;
using R2API;
using RoR2.UI;
using EntityStates.Events;
using MSU;
using SS2.Components;

namespace SS2
{
    public class StormController : NetworkBehaviour
    {
        #region Static Init
        public static StormController instance;
        public static PickupDropTable dropTable;
        
        public static bool debugObjective = false;
        private static Vector3 weatherBarLocalPosition = new Vector3(-15.6f, -105.6f, 0f);
        [SystemInitializer]
        static void Init()
        {
            StormController.dropTable = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/Chest1/dtChest1.asset").WaitForCompletion();
            On.RoR2.Run.InstantiateUi += InstantiateWeatherBar;
        }

        private static GameObject InstantiateWeatherBar(On.RoR2.Run.orig_InstantiateUi orig, Run self, Transform uiRoot)
        {
            GameObject uiInstance = orig(self, uiRoot);
            if (uiInstance)
            {
                var weatherBarPrefab = SS2Assets.LoadAsset<GameObject>("WeatherBar", SS2Bundle.Events);
                if (weatherBarPrefab)
                {
                    var weatherBarInstance = GameObject.Instantiate(weatherBarPrefab, uiInstance.transform);
                    weatherBarInstance.transform.localPosition = weatherBarLocalPosition;
                }
            }
            return uiInstance;
        }
        #endregion

        [SerializeField] private EntityStateMachine stormStateMachine;
        [SerializeField] private EntityStateMachine barStateMachine;

        [Serializable]
        public struct StormVFX
        {
            public R2API.DirectorAPI.StageSerde stageSerde;
            public string customStageName;
            public GameObject effectPrefab;
            public float cloudHeight;
        }
        // TODO: really dont think this information should be kept in StormController
        public StormVFX[] eventVFX = Array.Empty<StormVFX>();

        private float endLerpIntensity;
        private float startLerpIntensity;
        private float lerpTimeScale;
        private float lerpTime = 1f;
        private float effectIntensity;

        public IIntensityScaler[] intensityScalers;

        public int currentLevel { get; private set; }
        public int MaxStormLevel { get; private set; }
        public bool IsPermanent { get; private set; }
        public float levelPercentComplete { get; private set; }
        public float levelFractionComplete { get => levelPercentComplete * 0.01f; }

        public Xoroshiro128Plus timeRng = new Xoroshiro128Plus(0UL);
        public Xoroshiro128Plus treasureRng = new Xoroshiro128Plus(0UL);
        [NonSerialized]
        public Run.FixedTimeStamp stormStartTime;

        [NonSerialized]
        public bool hasStarted;

        [SyncVar]
        private float chargeInCurrentLevel;
        private void OnEnable()
        {
            if (StormController.instance)
            {
                Debug.LogError("Only one StormController can exist at a time.");
                Destroy(base.gameObject);
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
        private void Awake()
        {
            this.stormStateMachine = base.GetComponent<EntityStateMachine>();
        }

        private void Start()
        {
            InstantiateEffect();         
            this.SetEffectIntensity(this.effectIntensity);

            // TODO: reevaluate rng usage
            timeRng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
            treasureRng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);

            // TODO: dont use difficulty scaling value
            DifficultyDef difficulty = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
            MaxStormLevel = Mathf.FloorToInt(difficulty.scalingValue) + 1;
            MaxStormLevel = Mathf.Clamp(MaxStormLevel, 2, 4); // drizzle 2, monsoon/typhoon+ 4

            IsPermanent = Run.instance.GetEventFlag("PermanentStorms");

            #if DEBUG
            debugObjective = true;
            #endif
            if (debugObjective)
                ObjectivePanelController.collectObjectiveSources += StormObjective;        
        }

        private void OnDestroy()
        {
            ObjectivePanelController.collectObjectiveSources -= StormObjective;
        }

        // TODO: implement this
        //[Serializable]
        //public struct StormLevel
        //{
        //    public EntityStates.SerializableEntityStateType stateType;
        //    public Sprite icon;
        //}

        // TODO: not this !!
        public Sprite GetWeatherIcon(int level)
        {
            string path = "texIconStorm" + level;
            var icon = SS2Assets.LoadAsset<Sprite>(path, SS2Bundle.Events);
            if (!icon) SS2Log.Error($"Could not find Sprite `{path}`");
            return icon;
        }
        // TODO: Figure out when to start the animation. We want it slightly inaccurate (offset by a couple seconds).
        public void StartBarAnimation(float newIntensity, float duration = -1f)
        {
            if (barStateMachine)
            {
                // TODO: Figure out which values to pass in, or if AnimateWeatherBar should calculate it itself.
                barStateMachine.SetNextState(new AnimateWeatherBar());
            }
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
            newIntensity *= (Storm.EffectIntensity.value / 100f);
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
                this.stormStateMachine.SetNextState(new EntityStates.Events.Storm { stormLevel = stormLevel, lerpDuration = 5f });
            }
            else
            {
                this.stormStateMachine.SetNextState(new Calm());
            }

            this.currentLevel = stormLevel;
        }

        public void OnStormLevelCompleted()
        {
            hasStarted = true;
            this.currentLevel++;
        }

        public void InstantiateEffect()
        {
            GameObject effectPrefab = SS2Assets.LoadAsset<GameObject>("DefaultThunderstormEffect", SS2Bundle.Events);
            float cloudHeight = 100f;
            foreach(StormVFX vfx in this.eventVFX)
            {
                var stageFlags = (DirectorAPI.Stage)vfx.stageSerde;
                if (stageFlags.HasFlag(DirectorAPI.GetStageEnumFromSceneDef(SceneCatalog.GetSceneDefForCurrentScene()))) // im gonna puke
                {
                    effectPrefab = vfx.effectPrefab;
                    cloudHeight = vfx.cloudHeight;
                    break;
                }
            }
            GameObject effectInstance = GameObject.Instantiate(effectPrefab, base.gameObject.transform);
            this.intensityScalers = effectInstance.GetComponentsInChildren<IIntensityScaler>(true);
            Transform cloud = effectInstance.GetComponent<ChildLocator>()?.FindChild("CloudLayer");
            if(cloud)
            {
                cloud.gameObject.SetActive(true);
                cloud.position = Vector3.up * cloudHeight;
            }
        }


        // START STORM 2!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [ConCommand(commandName = "start_storm", flags = ConVarFlags.Cheat | ConVarFlags.ExecuteOnServer, helpText = "Sets the current storm level. Zero to disable. Format: {stormLevel}")]
        public static void CCSetStormLevel(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            int level = args.GetArgInt(0);
            StormController stormController = StormController.instance;
            if (!stormController)
            {
                stormController = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StormController", SS2Bundle.Events)).GetComponent<StormController>();

                var evt = stormController.GetComponent<GameplayEvent>();
                evt.doNotAnnounceEnd = true;
                evt.doNotAnnounceStart = true;

                NetworkServer.Spawn(stormController.gameObject);
            }
            stormController.ForceStormLevel(level);
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
                return string.Format("effectIntensity: {0} hehe storm level {1}, {2}%", intensity, instance.currentLevel, Mathf.FloorToInt(instance.levelPercentComplete));
            }

            public override bool IsDirty()
            {
                return true;
            }
        }
    }
}

