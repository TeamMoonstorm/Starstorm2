using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2
{
    public class BlitzRunBehavior : MonoBehaviour
    {
        private static BlitzRunBehavior instance;

        private float intervalStopwatch;
        private float gameOverStopwatch;
        private bool isGameOver;

        private const float eventInterval = 120f; // 2 minutes
        private const float blitzDuration = 600f; // 10 minutes

        private Xoroshiro128Plus rng;

        private static readonly DateTime seedEpoch = new DateTime(2018, 8, 27, 0, 0, 0, DateTimeKind.Utc);

        private void OnEnable()
        {
            instance = this;
            Run.onRunStartGlobal += OnRunStart;
            On.RoR2.Run.GenerateSeedForNewRun += OnGenerateSeedForNewRun;
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin += OnTeleporterInteractionBegin;
            On.RoR2.SceneDirector.PopulateScene += OnPopulateScene;
            ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
        }

        private void OnDisable()
        {
            Run.onRunStartGlobal -= OnRunStart;
            On.RoR2.Run.GenerateSeedForNewRun -= OnGenerateSeedForNewRun;
            On.RoR2.TeleporterInteraction.IdleState.OnInteractionBegin -= OnTeleporterInteractionBegin;
            On.RoR2.SceneDirector.PopulateScene -= OnPopulateScene;
            ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
            instance = null;
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active || isGameOver || rng == null)
                return;

            float dt = Time.fixedDeltaTime;

            intervalStopwatch += dt;
            if (intervalStopwatch >= eventInterval)
            {
                intervalStopwatch -= eventInterval;
                SpawnIntervalBoss();
            }

            gameOverStopwatch += dt;
            if (gameOverStopwatch >= blitzDuration)
            {
                isGameOver = true;
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "SS2_GAMEMODE_BLITZ_OVER"
                });

                SceneDef moonScene = SceneCatalog.FindSceneDef("moon2");
                if (moonScene != null && Stage.instance)
                {
                    Stage.instance.BeginAdvanceStage(moonScene);
                }
                else
                {
                    SS2Log.Warning("BlitzRunBehavior: Could not find moon2 scene or Stage.instance is null.");
                }
            }
        }

        private ulong OnGenerateSeedForNewRun(On.RoR2.Run.orig_GenerateSeedForNewRun orig, Run self)
        {
            if (self.TryGetComponent<BlitzRunBehavior>(out _))
            {
                uint dayCycle = (uint)((DateTime.UtcNow - seedEpoch).Days);
                return (ulong)dayCycle << 32;
            }
            return orig(self);
        }

        private void OnRunStart(Run run)
        {
            if (run.gameObject != gameObject)
                return;

            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(run.seed);

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "SS2_GAMEMODE_BLITZ_START"
                });
            }
        }

        private void SpawnIntervalBoss()
        {
            if (!ClassicStageInfo.instance)
            {
                SS2Log.Warning("BlitzRunBehavior: No ClassicStageInfo, cannot spawn boss.");
                return;
            }

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "SS2_GAMEMODE_BLITZ_INTERVAL"
            });

            WeightedSelection<DirectorCard> monsterSelection = ClassicStageInfo.instance.monsterSelection;
            if (monsterSelection == null || monsterSelection.Count == 0)
                return;

            // Filter to boss-tier (champion) enemies, same logic as CombatDirector.SetNextSpawnAsBoss
            WeightedSelection<DirectorCard> bossSelection = new WeightedSelection<DirectorCard>();
            for (int i = 0; i < monsterSelection.Count; i++)
            {
                var choice = monsterSelection.GetChoice(i);
                SpawnCard spawnCard = choice.value.GetSpawnCard();
                if (!spawnCard || !spawnCard.prefab)
                    continue;

                if (!spawnCard.prefab.TryGetComponent<CharacterMaster>(out var master) || !master.bodyPrefab)
                    continue;

                if (!master.bodyPrefab.TryGetComponent<CharacterBody>(out var body))
                    continue;

                bool isChampion = body.isChampion;
                bool forbiddenAsBoss = (spawnCard as CharacterSpawnCard)?.forbiddenAsBoss ?? false;

                if (isChampion && !forbiddenAsBoss && choice.value.IsAvailable())
                    bossSelection.AddChoice(choice);
            }

            if (bossSelection.Count == 0)
            {
                SS2Log.Warning("BlitzRunBehavior: No valid boss cards available for this stage.");
                return;
            }

            DirectorCard bossCard = bossSelection.Evaluate(rng.nextNormalizedFloat);

            // Pick a random alive player as spawn target
            GameObject spawnTarget = GetRandomAlivePlayerBody();
            if (!spawnTarget)
                return;

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = 10f,
                maxDistance = 40f,
                spawnOnTarget = spawnTarget.transform
            };

            DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(bossCard.GetSpawnCard(), placementRule, rng);
            spawnRequest.teamIndexOverride = TeamIndex.Monster;

            GameObject spawnedMaster = DirectorCore.instance?.TrySpawnObject(spawnRequest);
            if (!spawnedMaster)
            {
                SS2Log.Warning("BlitzRunBehavior: Boss spawn placement failed.");
                return;
            }

            if (spawnedMaster.TryGetComponent<CharacterMaster>(out var bossmaster))
            {
                // Give the boss extra health scaling based on difficulty
                float boostHp = 1f + (Run.instance.compensatedDifficultyCoefficient * 0.3f);
                bossmaster.inventory?.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt(boostHp));
            }
        }

        private GameObject GetRandomAlivePlayerBody()
        {
            ReadOnlyCollection<PlayerCharacterMasterController> players = PlayerCharacterMasterController.instances;
            List<GameObject> aliveBodies = new List<GameObject>();

            foreach (var pcmc in players)
            {
                if (pcmc.master && pcmc.master.hasBody)
                    aliveBodies.Add(pcmc.master.GetBodyObject());
            }

            if (aliveBodies.Count == 0)
                return null;

            return rng.NextElementUniform(aliveBodies);
        }

        private void OnTeleporterInteractionBegin(
            On.RoR2.TeleporterInteraction.IdleState.orig_OnInteractionBegin orig,
            EntityStates.BaseState self, Interactor activator)
        {
            if (instance)
                return;

            orig(self, activator);
        }

        private void OnPopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);

            if (!instance || !self.teleporterInstance)
                return;

            if (!self.teleporterInstance.TryGetComponent<InteractionProcFilter>(out var filter))
                filter = self.teleporterInstance.AddComponent<InteractionProcFilter>();

            filter.shouldAllowOnInteractionBeginProc = false;
        }

        private static void OnCollectObjectiveSources(
            CharacterMaster master,
            List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            if (!instance)
                return;

            objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
            {
                master = master,
                objectiveType = typeof(BlitzSurviveObjectiveTracker),
                source = instance
            });
        }

        public class BlitzSurviveObjectiveTracker : ObjectivePanelController.ObjectiveTracker
        {
            public override string GenerateString()
            {
                return Language.GetString("SS2_GAMEMODE_BLITZ_OBJECTIVE");
            }

            public override bool IsDirty()
            {
                return false;
            }
        }

        // TODO: Dont delete
        // On.RoR2.Run.HandlePlayerFirstEntryAnimation += OnHandlePlayerFirstEntryAnimation;
        // private void OnHandlePlayerFirstEntryAnimation(
        //     On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig,
        //     Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        // {
        //     if (self.gameObject == gameObject)
        //     {
        //         // No survivor pod — players teleport in directly
        //         body.SetBodyStateToPreferredInitialState();
        //         return;
        //     }
        //     orig(self, body, spawnPosition, spawnRotation);
        // }
    }
}
