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
        private static int baseBossCredit = 8;
        private static float bossWaitDuration = 4.5f;
        private static float warningProgress = 75;
        private static readonly WeightedSelection<DirectorCard> cardSelector = new WeightedSelection<DirectorCard>(8);

        public DirectorCardCategorySelection interactableDirectorCards;
        public int baseInteractableCredit = 150;

        public EquipmentDef bossEliteEquipment;
        public ItemDef bossDrop;
        public string eliteEquipmentAddress;
        public EquipmentDef eliteEquipment;
        public Transform eliteParticles;

        [Header("Tokens")]
        public string startToken = "";
        public string warningToken = "";
        public string bossTriggerToken = "";
        public string endToken = "";

        private float bossCredit
        {
            get
            {
                return (float)baseBossCredit * Stage.instance.entryDifficultyCoefficient;
            }
        }

        [SyncVar]
        [NonSerialized]
        public int currentProgress;
        [NonSerialized]
        [SyncVar]
        public bool hasStarted;

        private CombatDirector bossDirector;
        private CombatSquad bossSquad;
        private DirectorCard chosenBossCard;
        private Xoroshiro128Plus rng;
        private float bossWaitStopwatch;
        private bool hasSpawnedBoss;
        private bool hasWarned;
        private bool hasTriggered;

        private void Awake()
        {
            if (NetworkServer.active)
            {
                if (!eliteEquipment && !string.IsNullOrWhiteSpace(eliteEquipmentAddress))
                {
                    eliteEquipment = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<EquipmentDef>(eliteEquipmentAddress).WaitForCompletion();
                }

                rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);
            }
            
        }
        private void Start()
        {       
            if(NetworkServer.active)
            {
                bossDirector = base.GetComponent<CombatDirector>();
                bossSquad = base.GetComponent<CombatSquad>();
                bossDirector.onSpawnedServer.AddListener(OnBossSpawned);
                bossSquad.onDefeatedServer += OnBossDefeatedServer;
                WeightedSelection<DirectorCard> weightedSelection = Util.CreateReasonableDirectorCardSpawnList(bossCredit, 1, 1);
                if (weightedSelection.Count > 0)
                {
                    chosenBossCard = weightedSelection.Evaluate(rng.nextNormalizedFloat);
                }
                else
                {
                    SS2Log.Error("Failed to find reasonable DirectorCard. too bad so sad. bossCredit = " + bossCredit);
                }

                PopulateScene();
            }
        }
        public void OnEnable()
        {
            EntityStates.Destructible.EliteCrystalDeath.onEliteCrystalDestroyed += OnEliteCrystalDestroyed;
        }


        private void OnDisable()
        {
            EntityStates.Destructible.EliteCrystalDeath.onEliteCrystalDestroyed -= OnEliteCrystalDestroyed;
        }

        private void PopulateScene()
        {
            while (this.baseInteractableCredit > 0)
            {
                var deck = interactableDirectorCards.GenerateDirectorCardWeightedSelection();
                DirectorCard directorCard = this.SelectCard(deck, this.baseInteractableCredit);
                if (directorCard == null)
                {
                    break;
                }
                if (directorCard.IsAvailable())
                {
                    this.baseInteractableCredit -= directorCard.cost;
                    if (Run.instance)
                    {
                        int i = 0;
                        while (i < 10)
                        {
                            DirectorPlacementRule placementRule = new DirectorPlacementRule
                            {
                                placementMode = DirectorPlacementRule.PlacementMode.Random,
                            };
                            GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(directorCard.GetSpawnCard(), placementRule, this.rng));
                            if (gameObject)
                            {
                                PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                                if (component && component.costType == CostTypeIndex.Money)
                                {
                                    component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                                }
                                break;
                            }
                            else
                            {
                                i++;
                            }
                        }
                    }
                }
            }
        }
        private DirectorCard SelectCard(WeightedSelection<DirectorCard> deck, int maxCost)
        {
            EliteEventMissionController.cardSelector.Clear();
            int i = 0;
            int count = deck.Count;
            while (i < count)
            {
                WeightedSelection<DirectorCard>.ChoiceInfo choice = deck.GetChoice(i);
                if (choice.value.cost <= maxCost)
                {
                    EliteEventMissionController.cardSelector.AddChoice(choice);
                }
                i++;
            }
            if (EliteEventMissionController.cardSelector.Count == 0)
            {
                return null;
            }
            return EliteEventMissionController.cardSelector.Evaluate(this.rng.nextNormalizedFloat);
        }

        private void OnEliteCrystalDestroyed(int progress)
        {
            if (NetworkServer.active)
            {
                currentProgress += progress;
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;

            if (!hasWarned && currentProgress >= warningProgress)
            {
                hasWarned = true;
                if (!string.IsNullOrEmpty(warningToken))
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = warningToken,
                    });
                }
            }

            if (!hasSpawnedBoss && currentProgress >= 100)
            {
                if (!hasTriggered)
                {
                    hasTriggered = true;
                    if (!string.IsNullOrEmpty(bossTriggerToken))
                    {
                        GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
                        {
                            eventToken = bossTriggerToken,
                            eventColor = bossEliteEquipment.passiveBuffDef.buffColor,
                            textDuration = 6,
                        };
                        GameplayEventTextController.instance.EnqueueNewTextRequest(request, true);
                    }
                }

                bossWaitStopwatch += Time.fixedDeltaTime;
                if (bossWaitStopwatch > bossWaitDuration)
                {
                    SpawnBoss();
                }
            }
        }

        public void SpawnBoss()
        {
            hasSpawnedBoss = true;

            bossDirector.enabled = true;
            bossDirector.monsterCredit += bossCredit;
            bossDirector.OverrideCurrentMonsterCard(chosenBossCard);
            bossDirector.monsterSpawnTimer = 0f;
        }

        public void OnBossSpawned(GameObject masterObject)
        {
            // TODO: REBALANCE !!!!!!!!!!!!!!!!!!!!!!!!!!!
            Inventory inventory = masterObject.GetComponent<Inventory>();
            if (inventory)
            {
                inventory.SetEquipmentIndexForSlot(bossEliteEquipment.equipmentIndex, 0, 0);
                int loopCount = Mathf.Max(Run.instance.loopClearCount, 0);
                inventory.GiveItemPermanent(SS2Content.Items.BoostCharacterSize, 100);
                inventory.GiveItemPermanent(SS2Content.Items.MaxHealthPerMinute, 2 + Run.instance.stageClearCount * loopCount * loopCount * (1 + EtherealBehavior.instance.etherealStagesCompleted));
            }
            CharacterBody body = masterObject.GetComponent<CharacterMaster>().GetBody();
            body.baseMaxHealth = Mathf.Max(body.maxHealth, 2100f);
            body.baseDamage = 20;
            body.PerformAutoCalculateLevelStats();           
            new FriendManager.SyncBaseStats(body).Send(R2API.Networking.NetworkDestination.Clients);
            if (bossDrop && bossDrop.itemIndex != ItemIndex.None)
            {
                body.gameObject.AddComponent<OnBossKilledServer>().drop = PickupCatalog.FindPickupIndex(bossDrop.itemIndex);
            }
            else
            {
                SS2Log.Error("Elite Event " + base.name + " has no boss drop"!);
            }
        }

        private void OnBossDefeatedServer()
        {

        }

        private string GetBossName()
        {
            string name = "Dummy";
            CharacterMaster component = chosenBossCard.spawnCard.prefab.GetComponent<CharacterMaster>();
            if (component)
            {
                CharacterBody component2 = component.bodyPrefab.GetComponent<CharacterBody>();
                if (component2)
                {
                    name = Util.GetBestBodyName(component.bodyPrefab);
                }
            }
            return name;
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
    }
}
