using JetBrains.Annotations;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Components
{
    public sealed class DropPodController : NetworkBehaviour, IInteractable, IDisplayNameProvider
    {
        public enum RewardType : int
        {
            Item = 1,
            Enemy = 2,
            Swarm = 3,
            Corpse = 4,
            Nothing = 5,
        }

        public string contextToken = "SS2_INTERACTABLE_DROPPOD_CONTEXT";

        public string displayNameToken = "SS2_INTERACTABLE_DROPPOD_NAME";

        public float itemWeight = 0.25f;
        public float enemyWeight = 0.5f;
        public float swarmWeight = 0.2f;
        public float corpseWeight = 0.05f;

        public Transform dropTransform; // set from DropPod.cs

        // itme drop values
        public BasicPickupDropTable itemDropTable; // set from DropPod.cs
        public float itemDropForwardVelocity = 4f;
        public float itemDropUpwardVelocity = 7f;

        // single monster values
        public int monsterCredit = 12;
        public int monsterGoldReward = 25;
        public float monsterExpRewardCoefficient = 0.2f;
        public float monsterSpawnUpVelocity = 7f;
        public float monsterSpawnForwardVelocity = 7f;

        // swarm values
        public int swarmMinSpawnCount = 3;
        public int swarmMaxSpawnCount = 8;
        public int swarmTotalGoldReward = 40;
        public float swarmExpRewardCoefficient = 0.1f;
        public float swarmSpawnUpVelocity = 18f;
        public float swarmSpawnForwardVelocity = 15f;
        public float swarmSpawnSpreadAngle = 130f;
        public float swarmSpawnVelocityVariance = 0.67f;

        private Xoroshiro128Plus rng;

        [SyncVar]
        private bool opened;

        private WeightedSelection<RewardType> rewardSelection = new WeightedSelection<RewardType>();

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
                rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                monsterGoldReward = (int)(monsterGoldReward * Run.instance.difficultyCoefficient);
                swarmTotalGoldReward = (int)(swarmTotalGoldReward * Run.instance.difficultyCoefficient);

                rewardSelection.AddChoice(RewardType.Item, itemWeight);
                rewardSelection.AddChoice(RewardType.Enemy, enemyWeight);
                rewardSelection.AddChoice(RewardType.Swarm, swarmWeight);
                rewardSelection.AddChoice(RewardType.Corpse, corpseWeight);
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
                opened = true;
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

        public bool ShouldProximityHighlight()
        {
            return false;
        }

        [Server]
        public RewardType GetRewardType()
        {
            return rewardSelection.Evaluate(rng.nextNormalizedFloat);
        }

        [Server]
        public void DropReward()
        {
            switch (GetRewardType())
            {
                case RewardType.Item:
                    SpawnItem(dropTransform);
                    break;
                case RewardType.Enemy:
                    SpawnEnemy(dropTransform);
                    break;
                case RewardType.Swarm:
                    SpawnSwarm(dropTransform);
                    break;
                case RewardType.Corpse:
                    SpawnCorpse(dropTransform);
                    break;
            }
        }

        [Server]
        public void SpawnItem(Transform exitSpot)
        {
            PickupIndex pickupReward = itemDropTable.GeneratePickup(this.rng).pickupIndex;
            if (pickupReward.isValid)
            {
                PickupDropletController.CreatePickupDroplet(pickupReward, exitSpot.position, Vector3.up * itemDropUpwardVelocity + exitSpot.transform.forward * itemDropForwardVelocity);
            }
            
        }

        [Server]
        public void SpawnEnemy(Transform exitSpot)
        {
            var directorCards = Util.CreateReasonableDirectorCardSpawnList(monsterCredit, 1, 1);

            if (directorCards.Count == 0)
            {
                SS2Log.Error("DropPodController.SpawnEnemy: No reasonable monster card found.");
                return;
            }

            DirectorCard dCard = directorCards.Evaluate(rng.nextNormalizedFloat);

            if (dCard != null)
            {
                DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(dCard.spawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    minDistance = 3,
                    maxDistance = 20,
                    spawnOnTarget = exitSpot,
                }, this.rng);
                spawnRequest.teamIndexOverride = TeamIndex.Monster;
                spawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(spawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate (SpawnCard.SpawnResult spawnResult)
                {
                    var body = spawnResult.spawnedInstance.GetComponent<CharacterMaster>().GetBody();
                    if (body)
                    {
                        var rewards = body.GetComponent<DeathRewards>();
                        if (rewards)
                        {
                            rewards.expReward = (uint)(dCard.cost * monsterExpRewardCoefficient * Run.instance.compensatedDifficultyCoefficient);
                            rewards.goldReward = (uint)monsterGoldReward;
                        }

                        foreach (EntityStateMachine entityStateMachine in body.GetComponents<EntityStateMachine>())
                        {
                            entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                        }

                        PhysForceInfo info = new PhysForceInfo();
                        info.force = exitSpot.transform.forward * monsterSpawnForwardVelocity + Vector3.up * monsterSpawnUpVelocity;
                        info.massIsOne = true;
                        info.ignoreGroundStick = true;

                        CharacterMotor motor = body.GetComponent<CharacterMotor>();
                        if (motor)
                        {
                            motor.Motor.ForceUnground(0.2f);

                            // applying force instantly is too early. wait until CharacterMotor starts
                            motor.onMotorStart += (cb) =>
                            {
                                cb.healthComponent.TakeDamageForce(info);
                            };
                        }

                        // fliers can be pushed instantly
                        if (body.isFlying)
                        {
                            info.force = exitSpot.transform.forward * monsterSpawnForwardVelocity * 1.3f;
                            body.healthComponent.TakeDamageForce(info);
                        }
                        
                    }
                });
                DirectorCore.instance?.TrySpawnObject(spawnRequest);
            }
        }

        [Server]
        public void SpawnSwarm(Transform exitSpot)
        {
            var directorCards = Util.CreateReasonableDirectorCardSpawnList(monsterCredit, 1, 1);

            if (directorCards.Count == 0)
            {
                SS2Log.Error("DropPodController.SpawnEnemy: No reasonable monster card found.");
                return;
            }

            DirectorCard dCard = directorCards.Evaluate(rng.nextNormalizedFloat);

            if (dCard != null)
            {
                int minSpawns = swarmMinSpawnCount;
                int maxSpawns = Mathf.Min(swarmMaxSpawnCount, (int)(monsterCredit * Run.instance.compensatedDifficultyCoefficient)); // smaller swarms early on, capped at swarmMaxSpawnCount later on
                int monsterCount = rng.RangeInt(minSpawns, maxSpawns+ 1);

                for (int i = 0; i < monsterCount; i++)
                {
                    DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(dCard.spawnCard, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        minDistance = 3,
                        maxDistance = 20,
                        spawnOnTarget = exitSpot,
                    }, this.rng);
                    spawnRequest.teamIndexOverride = TeamIndex.Monster;
                    spawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(spawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate (SpawnCard.SpawnResult spawnResult)
                    {
                        var body = spawnResult.spawnedInstance.GetComponent<CharacterMaster>().GetBody();
                        if (body)
                        {
                            var rewards = body.GetComponent<DeathRewards>();
                            if (rewards)
                            {
                                rewards.expReward = (uint)(dCard.cost * swarmExpRewardCoefficient * Run.instance.compensatedDifficultyCoefficient);
                                rewards.goldReward = (uint)(swarmTotalGoldReward / monsterCount);
                            }

                            foreach (EntityStateMachine entityStateMachine in body.GetComponents<EntityStateMachine>())
                            {
                                entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                            }


                            PhysForceInfo info = new PhysForceInfo();
                            info.force = exitSpot.transform.forward * swarmSpawnForwardVelocity + Vector3.up * swarmSpawnUpVelocity;
                            info.massIsOne = true;
                            info.ignoreGroundStick = true;

                            // apply random spread
                            info.force = Util.ApplySpread(info.force, 0f, swarmSpawnSpreadAngle, 1f, 0.5f);

                            // apply random total speed
                            float variance = rng.RangeFloat(1 / (1 + swarmSpawnVelocityVariance), 1 + swarmSpawnVelocityVariance);
                            info.force *= variance;

                            CharacterMotor motor = body.GetComponent<CharacterMotor>();
                            if (motor)
                            {
                                motor.Motor.ForceUnground(0.2f);

                                // applying force instantly is too early. wait until CharacterMotor starts
                                motor.onMotorStart += (cb) =>
                                {
                                    cb.healthComponent.TakeDamageForce(info);
                                };
                            }

                            // fliers can be pushed instantly
                            if (body.isFlying)
                            {
                                info.force = exitSpot.transform.forward * monsterSpawnForwardVelocity * 1.3f;
                                body.healthComponent.TakeDamageForce(info);
                            }
                        }
                    });
                    DirectorCore.instance?.TrySpawnObject(spawnRequest);
                }
                
            }
        }


        [Server]
        public void SpawnCorpse(Transform exitSpot)
        {
            var unlockedSurvivors = GetUnlockedSurvivors();
            var randomSurvivor = unlockedSurvivors[rng.RangeInt(0, unlockedSurvivors.Count)];
            var instancedSurvivor = Instantiate(randomSurvivor.bodyPrefab, exitSpot);
            var survivorHealthComponent = instancedSurvivor.GetComponent<HealthComponent>();
            if (survivorHealthComponent)
            {
                survivorHealthComponent.Suicide();
            }
            NetworkServer.Spawn(instancedSurvivor);
        }

        private List<SurvivorDef> GetUnlockedSurvivors()
        {
            List<SurvivorDef> unlockedSurvivors = new List<SurvivorDef>();
            var allSurvivors = SurvivorCatalog.survivorDefs;
            for (int i = 0; i < allSurvivors.Length; i++)
            {
                var survivorDef = allSurvivors[i];
                if (survivorDef && survivorDef.unlockableDef)
                {
                    if (Run.instance.IsUnlockableUnlocked(survivorDef.unlockableDef))
                    {
                        unlockedSurvivors.Add(survivorDef);
                    }
                }
            }

            return unlockedSurvivors;
        }

    }
}