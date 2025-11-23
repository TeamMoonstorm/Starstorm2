using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.UI;
using UnityEngine.Networking;
using MSU;
using R2API.Networking.Interfaces;
namespace SS2
{
    public class EliteEventMissionController : NetworkBehaviour
    {
        public EquipmentDef bossEliteEquipment;
        public ItemDef bossDrop;
        public string eliteEquipmentAddress;
        public EquipmentDef eliteEquipment;
        public Transform eliteParticles;
        public int baseBossCredit = 12;
        public float maxDuration = 180f;
        public float bossWaitDuration = 3f;
        private float bossCredit
        {
            get
            {
                return (float)this.baseBossCredit * Stage.instance.entryDifficultyCoefficient;
            }
        }
        [SyncVar]
        public int requiredEliteKills = 25;
        [SyncVar]
        [NonSerialized]
        public int currentEliteKills;
        [NonSerialized]
        [SyncVar]
        public bool hasStarted;

        
        private BuffIndex eliteBuffIndex;

        private GameplayEvent gameplayEvent;
        private CombatDirector bossDirector;
        private CombatSquad bossSquad;
        private DirectorCard chosenBossCard;
        private float stopwatch;
        private float bossWaitStopwatch;
        private bool hasSpawnedBoss;
        
        private Dictionary<CombatDirector, float> directorToOriginalEliteBias = new Dictionary<CombatDirector, float>();
        private void Awake()
        {
            if(!eliteEquipment && !string.IsNullOrWhiteSpace(eliteEquipmentAddress))
            {
                eliteEquipment = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<EquipmentDef>(eliteEquipmentAddress).WaitForCompletion();
                if(eliteEquipment && eliteEquipment.passiveBuffDef)
                {
                    eliteBuffIndex = eliteEquipment.passiveBuffDef.buffIndex;
                }
                else
                {
                    SS2Log.Error("Invalid Equipment " + eliteEquipmentAddress);
                }
                
            }
        }
        private void Start()
        {       
            if(NetworkServer.active)
            {
                gameplayEvent = base.GetComponent<GameplayEvent>();
                bossDirector = base.GetComponent<CombatDirector>();
                bossSquad = base.GetComponent<CombatSquad>();
                bossDirector.onSpawnedServer.AddListener(OnBossSpawned);
                bossSquad.onDefeatedServer += OnBossDefeatedServer;
                WeightedSelection<DirectorCard> weightedSelection = Util.CreateReasonableDirectorCardSpawnList(bossCredit, 1, 1);
                if (weightedSelection.Count > 0)
                {
                    chosenBossCard = weightedSelection.Evaluate(bossDirector.rng.nextNormalizedFloat);
                }
                else
                {
                    SS2Log.Error("Failed to find reasonable DirectorCard. too bad so sad. bossCredit = " + bossCredit);
                }
                StartEvent();
            }
        }
        public void OnEnable()
        {
            ObjectivePanelController.collectObjectiveSources += EliteObjective;
            TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginChargingGlobal;
        }

        

        private void OnDisable()
        {
            if (hasStarted)
                StopEvent();
            ObjectivePanelController.collectObjectiveSources -= EliteObjective;
            TeleporterInteraction.onTeleporterBeginChargingGlobal -= OnTeleporterBeginChargingGlobal;
        }
        public void StartEvent()
        {
            hasStarted = true;
            foreach (CombatDirector director in allCombatDirectors)
            {
                // apparently this makes monsters rarely ever spawn ??? idk
                //directorToOriginalEliteBias.Add(director, director.eliteBias);
                //director.eliteBias = Mathf.Infinity; // stop credits from being spent on elites
                director.onSpawnedServer.AddListener(OnSpawnedServer);
            }
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
            gameplayEvent.StartEvent();
                
        }
        public void StopEvent()
        {
            hasStarted = false;
            foreach (CombatDirector director in allCombatDirectors)
            {
                //director.eliteBias = directorToOriginalEliteBias.TryGetValue(director, out float eliteBias) ? eliteBias : 1;
                director.onSpawnedServer.RemoveListener(OnSpawnedServer);
            }
            GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
            gameplayEvent.EndEvent();
            
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;
            if (hasStarted)
            {
                stopwatch += Time.fixedDeltaTime;
                if (!hasSpawnedBoss && stopwatch >= maxDuration)
                {
                    StopEvent();
                    return;
                }
                if (!hasSpawnedBoss && currentEliteKills >= requiredEliteKills)
                {
                    bossWaitStopwatch += Time.fixedDeltaTime;
                    if (bossWaitStopwatch > bossWaitDuration)
                    {
                        SpawnBoss();
                    }
                }
                else
                    bossWaitStopwatch = 0;
            }
            else
                stopwatch = 0;           
        }

        public void SpawnBoss()
        {
            hasSpawnedBoss = true;

            bossDirector.enabled = true;
            bossDirector.monsterCredit += bossCredit;
            bossDirector.OverrideCurrentMonsterCard(chosenBossCard);
            bossDirector.monsterSpawnTimer = 0f;
            CharacterMaster component = chosenBossCard.spawnCard.prefab.GetComponent<CharacterMaster>();
            if (component)
            {
                CharacterBody component2 = component.bodyPrefab.GetComponent<CharacterBody>();
                if (component2)
                {
                    GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
                    {
                        eventToken = "yooooooooooo mama " + Util.GetBestBodyName(component.bodyPrefab),
                        eventColor = bossEliteEquipment.passiveBuffDef.buffColor,
                        textDuration = 6,
                    };
                    GameplayEventTextController.instance.EnqueueNewTextRequest(request, true);
                }
            }
        }

        public void OnBossSpawned(GameObject masterObject)
        {
            Inventory inventory = masterObject.GetComponent<Inventory>();
            if (inventory)
            {
                inventory.SetEquipmentIndex(bossEliteEquipment.equipmentIndex);
                int loopCount = Mathf.Max(Run.instance.loopClearCount, 0);
                inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 100);
                inventory.GiveItem(SS2Content.Items.MaxHealthPerMinute, 2 + Run.instance.stageClearCount * loopCount * loopCount * (1 + EtherealBehavior.instance.etherealStagesCompleted));
            }
            CharacterBody body = masterObject.GetComponent<CharacterMaster>().GetBody();
            body.baseMaxHealth = Mathf.Max(body.maxHealth, 2100f);
            body.baseDamage = 20;
            body.PerformAutoCalculateLevelStats();           
            new FriendManager.SyncBaseStats(body).Send(R2API.Networking.NetworkDestination.Clients);
            if (bossDrop && bossDrop.itemIndex != ItemIndex.None)
                body.gameObject.AddComponent<OnBossKilledServer>().drop = PickupCatalog.FindPickupIndex(bossDrop.itemIndex);
            else
            {
                SS2Log.Error("Elite Event " + base.name + " has no boss drop"!);
            }
        }

        private void OnBossDefeatedServer()
        {
            StopEvent(); //////////////////////
        }

        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.victimTeamIndex != TeamIndex.Player && damageReport.victimBody.HasBuff(eliteBuffIndex))
            {
                switch(damageReport.victimBody.hullClassification)
                {
                    case HullClassification.Human:
                        currentEliteKills++;
                        break;
                    case HullClassification.Golem:
                        currentEliteKills += 2;
                        break;
                    case HullClassification.BeetleQueen:
                        currentEliteKills += 3;
                        break;
                }
            }
        }
        private void OnTeleporterBeginChargingGlobal(TeleporterInteraction obj)
        {
            StopEvent(); // should we?
        }

        public void OnSpawnedServer(GameObject masterObject)
        {
            Inventory inventory = masterObject.GetComponent<Inventory>();
            if(inventory)
            {
                inventory.GiveItemPermanent(SS2Content.Items.MultiElite); // enables multiple buffs from elite equipments, enables multiple elite equipment displays, fixes elite overlays after first slot
                
                // TODO: Use the non-obsolete method here
                inventory.SetEquipmentIndexForSlot(eliteEquipment.equipmentIndex, (uint)inventory._equipmentStateSlots.Length); // set the last equipment slot
                
                int boostDamage = inventory.GetItemCountPermanent(RoR2Content.Items.BoostDamage) - 10;
                if (boostDamage > 0)
                    inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, boostDamage);
                int boostHp = inventory.GetItemCountPermanent(RoR2Content.Items.BoostHp) - 10;
                if (boostHp > 0)
                    inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, boostHp);
            }    
        }
        private class OnBossKilledServer : MonoBehaviour, IOnKilledServerReceiver
        {
            public PickupIndex drop = PickupIndex.none;
            public void OnKilledServer(DamageReport damageReport)
            {
                if (drop == PickupIndex.none) return;
                
                int playerCount = Run.instance.participatingPlayerCount;
                if(playerCount > 1)
                {
                    float angle = 360f / (float)playerCount;
                    Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    PickupIndex drop = PickupCatalog.FindPickupIndex(SS2Content.Items.ShardStorm.itemIndex);
                    int i = 0;
                    while (i < playerCount)
                    {
                        PickupDropletController.CreatePickupDroplet(drop, damageReport.victimBody.corePosition, vector);
                        i++;
                        vector = rotation * vector;
                    }
                }
                else
                {
                    PickupDropletController.CreatePickupDroplet(drop, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
                    
                
            }
        }

        #region ObjectiveTracker
        private void EliteObjective(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> dest)
        {
            if(hasStarted && requiredEliteKills > currentEliteKills)
            {
                dest.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = master,
                    objectiveType = typeof(EliteObjectiveTracker),
                    source = this
                });
            }       
        }

        private class EliteObjectiveTracker : ObjectivePanelController.ObjectiveTracker
        {
            private int eliteKills = -1;
            private int requiredEliteKills = 25;
            public override string GenerateString()
            {
                EliteEventMissionController mission = (EliteEventMissionController)sourceDescriptor.source;
                eliteKills = mission.currentEliteKills;
                requiredEliteKills = mission.requiredEliteKills;
                return string.Format("elite kills heehaw: {0}%", (float)mission.currentEliteKills / (float)mission.requiredEliteKills * 100f);
            }
            public override bool shouldConsiderComplete => IsComplete();
            bool IsComplete()
            {
                return eliteKills != -1 ? eliteKills >= requiredEliteKills : base.shouldConsiderComplete;               
            }

            public override bool IsDirty()
            {
                return ((EliteEventMissionController)sourceDescriptor.source).currentEliteKills != eliteKills;
            }
        }
        #endregion

        #region Init
        [SystemInitializer]
        private static void Init()
        {
            On.RoR2.CombatDirector.Awake += (orig, self) =>
            {
                orig(self);
                if(NetworkServer.active)
                {
                    allCombatDirectors.Add(self);
                    self.gameObject.AddComponent<OnDestroyCallback>().onDestroy += () => { allCombatDirectors.Remove(self); };
                }            
            };
        }

        private class OnDestroyCallback : MonoBehaviour
        {
            public Action onDestroy;
            private void OnDestroy()
            {
                onDestroy?.Invoke();
            }
        }

        // list of all combatdirectors, including disabled
        private static List<CombatDirector> allCombatDirectors = new List<CombatDirector>();
        #endregion
    }
}
