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
        
        public static bool debug = false;

        private static Vector3 weatherBarLocalPositionDefault = new Vector3(-15.5f, -106.9f, 0f);

        public static StormVFX defaultStormVFX { get; private set; }

        [SystemInitializer]
        static void Init()
        {
            #if DEBUG
            debug = true;
            #endif

            StormController.dropTable = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/Chest1/dtChest1.asset").WaitForCompletion();

            defaultStormVFX = new StormVFX
            {
                effectPrefab = SS2Assets.LoadAsset<GameObject>("DefaultThunderstormEffect", SS2Bundle.Events),
                weatherBarPrefab = SS2Assets.LoadAsset<GameObject>("WeatherBar", SS2Bundle.Events),
                eventStartToken = "SS2_EVENT_THUNDERSTORM_START",
                eventEndToken = "SS2_EVENT_THUNDERSTORM_END",
            };
        }
        [ConCommand(commandName = "storm_debug", flags = (ConVarFlags.Cheat), helpText = "Toggle Storm debugging.")]
        private static void CCToggleDebug(ConCommandArgs args)
        {
            debug = !debug;
        }
        #endregion

        [SerializeField] private EntityStateMachine stormStateMachine;
        [SerializeField] private EntityStateMachine barStateMachine;

        [Serializable]
        public struct StormVFX
        {
            public R2API.DirectorAPI.StageSerde stageSerde;

            [Header("VFX")]
            public string customStageName;
            public GameObject effectPrefab;
            public float cloudHeight;

            [Header("UI")]
            public GameObject weatherBarPrefab;
            public string eventStartToken; // idk
            public string eventEndToken; 
        }
        // TODO: really dont think this information should be kept in StormController
        public StormVFX[] eventVFX = Array.Empty<StormVFX>();
        private StormVFX stormVFX;

        private float endLerpIntensity;
        private float startLerpIntensity;
        private float lerpTimeScale;
        private float lerpTime = 1f;
        private float effectIntensity;

        [NonSerialized] public IIntensityScaler[] intensityScalers;

        public int currentLevel { get; private set; }
        public int MaxStormLevel { get; private set; }
        public bool IsPermanent { get; private set; }
        public float chargeInCurrentLevel { get; private set; }
        public bool shouldActivateHud { get => _shouldActivateHud ; private set => _shouldActivateHud = value; } // exists for WeatherBarAnimator to listen to. seems weird

        private bool _shouldActivateHud;

        public Xoroshiro128Plus timeRng = new Xoroshiro128Plus(0UL);
        public Xoroshiro128Plus treasureRng = new Xoroshiro128Plus(0UL);
        public Xoroshiro128Plus ballRng = new Xoroshiro128Plus(0UL);

        public Run.FixedTimeStamp stormStartTime 
        { 
            get => new Run.FixedTimeStamp(_stormStartTime); 
            set => _stormStartTime = value.t; 
        }
        [SyncVar]
        private float _stormStartTime; //  The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.

        [NonSerialized]
        public bool hasStarted;

        private void OnEnable()
        {
            if (StormController.instance)
            {
                SS2Log.Error("Only one StormController can exist at a time.");
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
            ballRng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);

            // TODO: dont use difficulty scaling value
            DifficultyDef difficulty = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
            MaxStormLevel = Mathf.FloorToInt(difficulty.scalingValue) + 1;
            MaxStormLevel = Mathf.Clamp(MaxStormLevel, 2, 3);

            IsPermanent = Run.instance.GetEventFlag("PermanentStorms");

            ObjectivePanelController.collectObjectiveSources += StormObjective;        
        }

        private void OnDestroy()
        {
            ObjectivePanelController.collectObjectiveSources -= StormObjective;
        }

        public string GetStartToken()
        {
            return stormVFX.eventStartToken;
        }
        public string GetEndToken()
        {
            return stormVFX.eventEndToken;
        }

        // TODO: Figure out when to start the animation. We want it slightly inaccurate (offset by a couple seconds).
        public void StartBarAnimation(float newIntensity, float? duration = null)
        {
            if (barStateMachine)
            {
                // TODO: Figure out which values to pass in, or if AnimateWeatherBar should calculate it itself.
                barStateMachine.SetNextState(new AnimateWeatherBar());
            }
        }
        // ??
        public void SetBarActive(bool active)
        {
            shouldActivateHud = active;
        }

        public void StartEffectIntensityLerp(float newIntensity, float lerpDuration)
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

        // TODO: Figure out how storms progress and report that progression
        [NonSerialized] public float progressInCurrentLevel = 0f;
        private void FixedUpdate()
        {
            chargeInCurrentLevel = Mathf.Clamp(progressInCurrentLevel, 0, 100);

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

        public void SetStormLevel(int level)
        {
            if (!hasStarted)
            {
                hasStarted = level > 0;
            }
            currentLevel = level;
        }

        public void InstantiateEffect()
        {
            stormVFX = defaultStormVFX;
            var currentStage = DirectorAPI.GetStageEnumFromSceneDef(SceneCatalog.GetSceneDefForCurrentScene());

            // Stage VFX
            foreach (StormVFX vfx in this.eventVFX)
            {
                var stageFlags = (DirectorAPI.Stage)vfx.stageSerde;
                if (stageFlags.HasFlag(currentStage)) // im gonna puke
                {
                    stormVFX = vfx;
                    break;
                }
            }
            if (stormVFX.effectPrefab)
            {
                GameObject effectInstance = GameObject.Instantiate(stormVFX.effectPrefab, base.gameObject.transform);
                this.intensityScalers = effectInstance.GetComponentsInChildren<IIntensityScaler>(true);
                Transform cloud = effectInstance.GetComponent<ChildLocator>()?.FindChild("CloudLayer");
                if (cloud)
                {
                    cloud.gameObject.SetActive(true);
                    cloud.position = Vector3.up * stormVFX.cloudHeight;
                }
            }

            // Weather Bar
            for (int i = 0; i < Run.instance.uiInstances.Count; i++)
            {
                var uiInstance = Run.instance.uiInstances[i];
                if (uiInstance)
                {
                    if (stormVFX.weatherBarPrefab)
                    {
                        var weatherBarInstance = GameObject.Instantiate(stormVFX.weatherBarPrefab, uiInstance.transform);
                        ((RectTransform)weatherBarInstance.transform).anchoredPosition = weatherBarLocalPositionDefault; // TODO: UI mod support

                        // set sibling index to directly after DifficultyBar. this matters for ui layering
                        Transform difficultyBar = uiInstance.transform.Find("DifficultyBar");
                        if (difficultyBar)
                        {
                            int siblingIndex = difficultyBar.GetSiblingIndex();
                            weatherBarInstance.transform.SetSiblingIndex(siblingIndex + 1);
                        }
                    }
                    else
                    {
                        SS2Log.Error("WeatherBar prefab is null!");
                    }
                }
            }
        }

        // START STORM 2!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [ConCommand(commandName = "start_storm", flags = ConVarFlags.Cheat | ConVarFlags.ExecuteOnServer, helpText = "Sets the current storm level. Zero to disable. Format: start_storm {stormLevel}")]
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
            if (debug)
            {
                dest.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = master,
                    objectiveType = typeof(StormObjectiveTracker),
                    source = base.gameObject
                });
            }
        }

        private class StormObjectiveTracker : ObjectivePanelController.ObjectiveTracker
        {
            public override string GenerateString()
            {
                StormController instance = StormController.instance;
                float intensity = (float)Math.Round(instance.effectIntensity, 1);
                return string.Format("effectIntensity: {0} hehe storm level {1}, {2}%", intensity, instance.currentLevel, Mathf.FloorToInt(instance.chargeInCurrentLevel * 100f));
            }

            public override bool IsDirty()
            {
                return true;
            }
        }

        [ConCommand(commandName = "balllllightning", flags = (ConVarFlags.Cheat), helpText = "Spawns ball.")]
        private static void CCSpawnBall(ConCommandArgs args)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            Transform spawnTarget = null;
            DirectorPlacementRule.PlacementMode placementMode = DirectorPlacementRule.PlacementMode.Random;

            var rng = new Xoroshiro128Plus(0L);

            List<GameObject> playerBodyObjects = new List<GameObject>();
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController._instances)
            {
                if (player.master.hasBody)
                {
                    playerBodyObjects.Add(player.master.GetBodyObject());
                }
            }
            var playerBodyObject = rng.NextElementUniform(playerBodyObjects);
            if (playerBodyObject)
            {
                spawnTarget = playerBodyObject.transform;
                placementMode = DirectorPlacementRule.PlacementMode.Approximate;
            }

            var ballSpawnCard = SS2Assets.LoadAsset<InteractableSpawnCard>("iscBallLightningPickup", SS2Bundle.Events);
            DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(ballSpawnCard, new DirectorPlacementRule
            {
                minDistance = 10f,
                maxDistance = 120f,
                placementMode = placementMode,
                spawnOnTarget = spawnTarget,
            }, rng));
        }
    }
}

