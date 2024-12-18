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
        public Transform eliteParticles;
        public int baseBossCredit = 20;
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
        [SyncVar(hook = "OnEliteChanged")]
        [NonSerialized]
        public uint eliteEquipmentIndexPlusOne;

        private GameplayEvent gameplayEvent;
        private CombatDirector bossDirector;
        private DirectorCard chosenBossCard;
        private float stopwatch;
        private float bossWaitStopwatch;
        private bool hasSpawnedBoss;
        private BuffIndex eliteBuffIndex;
        private Dictionary<CombatDirector, float> directorToOriginalEliteBias = new Dictionary<CombatDirector, float>();

        private void Awake()
        {
            if(NetworkServer.active)
            {
                gameplayEvent = base.GetComponent<GameplayEvent>();
                bossDirector = base.GetComponent<CombatDirector>();
                bossDirector.onSpawnedServer.AddListener(OnBossSpawned);
                bossDirector.combatSquad.onDefeatedServer += OnBossDefeatedServer;
            }
        }
        private void Start()
        {
            if(NetworkServer.active)
            {
                WeightedSelection<DirectorCard> weightedSelection = Util.CreateReasonableDirectorCardSpawnList(bossCredit, 1, 1);
                if (weightedSelection.Count > 0)
                {
                    chosenBossCard = weightedSelection.Evaluate(bossDirector.rng.nextNormalizedFloat);
                }
                else
                {
                    SS2Log.Error("Failed to find reasonable DirectorCard. too bad so sad. bossCredit = " + bossCredit);
                }
            }
        }
        public void OnEnable()
        {
            ObjectivePanelController.collectObjectiveSources += EliteObjective;
        }
        private void OnDisable()
        {
            if (hasStarted)
                StopEvent();
            ObjectivePanelController.collectObjectiveSources -= EliteObjective;
        }
        public void StartEvent()
        {
            hasStarted = true;
            if(NetworkServer.active)
            {
                foreach (CombatDirector director in allCombatDirectors)
                {
                    directorToOriginalEliteBias.Add(director, director.eliteBias);
                    director.eliteBias = Mathf.Infinity; // stop credits from being spent on elites
                    director.onSpawnedServer.AddListener(OnSpawnedServer);
                }
                GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
                gameplayEvent.StartEvent();
            }       
        }
        public void StopEvent()
        {
            hasStarted = false;
            if (NetworkServer.active)
            {
                foreach (CombatDirector director in allCombatDirectors)
                {
                    director.eliteBias = directorToOriginalEliteBias.TryGetValue(director, out float eliteBias) ? eliteBias : 1;
                    director.onSpawnedServer.RemoveListener(OnSpawnedServer);
                }
                GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
                gameplayEvent.EndEvent();
            }
        }

        private void FixedUpdate()
        {
            if (hasStarted)
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= maxDuration)
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
                int loopCount = Mathf.Max(Run.instance.loopClearCount, 1);
                inventory.GiveItem(SS2Content.Items.BoostCharacterSize, 100);
                inventory.GiveItem(SS2Content.Items.MaxHealthPerMinute, 3 + loopCount * loopCount);
            }
            CharacterBody body = masterObject.GetComponent<CharacterMaster>().GetBody();
            body.maxHealth = 2100;
            body.damage = 12;
            new FriendManager.SyncBaseStats(body).Send(R2API.Networking.NetworkDestination.Clients);
        }

        private void OnBossDefeatedServer()
        {
            StopEvent();
        }

        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.victimTeamIndex != TeamIndex.Player && damageReport.victimBody.HasBuff(eliteBuffIndex))
            {
                currentEliteKills++;
            }
        }      

        public void OnSpawnedServer(GameObject masterObject)
        {
            Inventory inventory = masterObject.GetComponent<Inventory>();
            if(inventory)
            {
                inventory.GiveItem(SS2Content.Items.MultiElite); // enables multiple buffs from elite equipments, enables multiple elite equipment displays, fixes elite overlays after first slot
                inventory.SetEquipmentIndexForSlot((EquipmentIndex)eliteEquipmentIndexPlusOne, (uint)inventory.equipmentStateSlots.Length); // set the last equipment slot
                int boostDamage = inventory.GetItemCount(RoR2Content.Items.BoostDamage) - 10;
                if (boostDamage > 0)
                    inventory.GiveItem(RoR2Content.Items.BoostDamage, boostDamage);
                int boostHp = inventory.GetItemCount(RoR2Content.Items.BoostHp) - 10;
                if (boostHp > 0)
                    inventory.GiveItem(RoR2Content.Items.BoostDamage, boostHp);
            }    
        }

        public void OnEliteChanged(uint eliteEquipmentIndexPlusOne)
        {
            EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)eliteEquipmentIndexPlusOne - 1);
            if(equipmentDef && equipmentDef.passiveBuffDef)
            {
                eliteBuffIndex = equipmentDef.passiveBuffDef.buffIndex;
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
            public override string GenerateString()
            {
                EliteEventMissionController mission = (EliteEventMissionController)sourceDescriptor.source;
                eliteKills = mission.currentEliteKills;
                return string.Format("elite kills heehaw: {0} / {1}", mission.currentEliteKills, mission.requiredEliteKills);
            }
            public override bool shouldConsiderComplete => base.shouldConsiderComplete;
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
