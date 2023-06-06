using JetBrains.Annotations;
using RoR2;
using RoR2.Networking;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    public sealed class DropPodController : NetworkBehaviour, IInteractable, IDisplayNameProvider
    {
        public enum RewardType : int
        {
            Item = 1,
            Enemy = 2,
            Corpse = 3,
            Nothing = 4,
        }

        public string contextToken = "SS2_INTERACTABLE_DROPPOD_CONTEXT";

        public string displayNameToken = "SS2_INTERACTABLE_DROPPOD_NAME";

        public TeamIndex activatorEnemyTeamIndex;

        internal static DirectorCard[] currentStageMonsters;

        private PickupIndex[] currentStageWhites;

        private Xoroshiro128Plus rng;

        [SyncVar]
        private bool opened;

        private WeightedSelection<RewardType> rewardSelection = new WeightedSelection<RewardType>();

        public bool NetworkOpened
        {
            get
            {
                return opened;
            }
            [param: In]
            set
            {
                SetSyncVar(value, ref opened, 1u);
            }
        }

        public void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        public void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        private void Start()
        {
            if (Run.instance)
            {
                rng = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
                currentStageWhites = Run.instance.availableTier1DropList.ToArray();
                rewardSelection.AddChoice(RewardType.Item, 0.2f);
                rewardSelection.AddChoice(RewardType.Enemy, 0.5f);
                rewardSelection.AddChoice(RewardType.Corpse, 0.1f);
            }
        }

        public string GetContextString([NotNull] Interactor activator)
        {
            return Language.GetString(contextToken);
        }

        public string GetDisplayName()
        {
            return Language.GetString(displayNameToken);
        }

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            if (opened)
            {
                return Interactability.Disabled;
            }
            return Interactability.Available;
        }

        [Server]
        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            if (!opened)
            {
                NetworkOpened = true;
                EntityStateMachine stateMachine = GetComponent<EntityStateMachine>();
                CharacterBody activatorBody = activator.GetComponent<CharacterBody>();
                if (stateMachine && activatorBody)
                {
                    stateMachine.SetNextState(new EntityStates.DropPod.PreRelease());
                }
            }
        }

        public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator)
        {
            return false;
        }

        public bool ShouldShowOnScanner()
        {
            return !opened;
        }

        public override int GetNetworkChannel()
        {
            return QosChannelIndex.defaultReliable.intVal;
        }

        [Server]
        public RewardType GetRewardType()
        {
            return rewardSelection.Evaluate(rng.nextNormalizedFloat);
        }

        [Server]
        public void SpawnItem(Transform exitSpot)
        {
            PickupIndex pickupReward = currentStageWhites[rng.RangeInt(0, currentStageWhites.Length)];
            PickupDropletController.CreatePickupDroplet(pickupReward, exitSpot.position, Vector3.up * 3);
        }

        [Server]
        public void SpawnEnemy(Transform exitSpot)
        {

            DirectorCard dCard = currentStageMonsters[rng.RangeInt(0, currentStageMonsters.Length)]; //problem line

            if (dCard != null)
            {
                DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(dCard.spawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    minDistance = 3,
                    maxDistance = 20,
                    spawnOnTarget = exitSpot,
                }, RoR2Application.rng);
                spawnRequest.teamIndexOverride = TeamIndex.Monster;
                spawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(spawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate (SpawnCard.SpawnResult spawnResult)
                {
                    var body = spawnResult.spawnedInstance.GetComponent<CharacterMaster>().GetBody();
                    if (body)
                    {
                        var rewards = body.GetComponent<DeathRewards>();
                        if (rewards)
                        {
                            rewards.expReward = (uint)(dCard.cost * 0.2f * Run.instance.compensatedDifficultyCoefficient);
                            rewards.goldReward = (uint)(dCard.cost * 0.2 * 2 * Run.instance.compensatedDifficultyCoefficient);
                        }
                    }
                });
                DirectorCore.instance?.TrySpawnObject(spawnRequest);
            }
        }

        [Server]
        public void SpawnCorpse(Transform exitSpot)
        {
            var randomSurvivor = SurvivorCatalog.survivorDefs[rng.RangeInt(0, SurvivorCatalog.survivorDefs.Length)];
            var instancedSurvivor = Instantiate(randomSurvivor.bodyPrefab, exitSpot);
            var survivorHealthComponent = instancedSurvivor.GetComponent<HealthComponent>();
            if (survivorHealthComponent)
            {
                survivorHealthComponent.Suicide();
            }
            NetworkServer.Spawn(instancedSurvivor);
        }
    }
}