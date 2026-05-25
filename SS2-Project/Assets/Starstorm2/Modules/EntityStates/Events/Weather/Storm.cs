using SS2.Components;
using RoR2;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using SS2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using System;

namespace EntityStates.Events
{
    public abstract class BaseWeatherState : EntityStates.GameplayEvents.GameplayEventState
    {
        protected StormController stormController;
        public override void OnEnter()
        {
            base.OnEnter();
            this.stormController = base.GetComponent<StormController>();
        }
        public void SetBuffCountGlobal(int count)
        {
            var mask = TeamMask.GetEnemyTeams(TeamIndex.Player);
            mask.RemoveTeam(TeamIndex.Neutral);
            
            for (int i = 0; i < CharacterBody.readOnlyInstancesList.Count; i++)
            {
                var body = CharacterBody.readOnlyInstancesList[i];
                if (body)
                {
                    var team = body.teamComponent.teamIndex;
                    if (mask.HasTeam(team) && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
                    {
                        body.SetBuffCount(SS2Content.Buffs.BuffStorm.buffIndex, count);
                    }
                }
            }
        }
    }
    public class Calm : BaseWeatherState
    {
        public float duration;

        private static Color textColor = Color.gray;
        private static float warningDuration = 8.5f;

        private static float hudRevealDuration = 4f;

        private bool hasWarned;
        private bool hasRevealed;
        public override void OnEnter()
        {
            base.OnEnter();

            duration = stormController.stormStartTime - Run.FixedTimeStamp.now; // bro

            stormController.SetStormLevel(0); // why is the entitystate doing this??
            stormController.StartEffectIntensityLerp(0, 8f);
            stormController.StartBarAnimation(0f, 0f); // TODO: figure out what to put in this stupdi method
            stormController.SetBarActive(false); // probably want to hide it later since storm fx is still lerping away?

            if (stormController.hasStarted)
            {
                if (TeleporterUpgradeController.instance)
                {
                    TeleporterUpgradeController.instance.UpgradeStorm(false);
                }
                GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
                {
                    eventToken = stormController.GetEndToken(),
                    eventColor = textColor,
                    textDuration = 6,
                };
                GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);
            }

            if (NetworkServer.active)
            {
                SetBuffCountGlobal(0);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!ShouldCharge()) // ??
            {
                return;
            }

            stormController.progressInCurrentLevel = fixedAge / duration; // TODO: stupid fuck.

            Run.FixedTimeStamp warnTime = stormController.stormStartTime - warningDuration;
            if (warnTime.hasPassed && !hasWarned)
            {
                hasWarned = true;
                EnqueueWarningText();
            }

            Run.FixedTimeStamp revealTime = stormController.stormStartTime - hudRevealDuration;
            if (revealTime.hasPassed && !hasRevealed)
            {
                hasRevealed = true;
                stormController.SetBarActive(true);
            }

            if (isAuthority)
            {
                if (stormController.stormStartTime.hasPassed)
                {
                    outer.SetNextState(new Storm { stormLevel = 1, lerpDuration = 15f });
                }
            }
        }

        public void EnqueueWarningText()
        {
            hasWarned = true;
            GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
            {
                eventToken = stormController.GetStartToken(),
                eventColor = textColor,
                textDuration = 6,
            };
            GameplayEventTextController.instance.EnqueueNewTextRequest(request, true);
        }

        private bool ShouldCharge()
        {
            bool shouldCharge = !TeleporterInteraction.instance;
            shouldCharge |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle;
            shouldCharge |= !stormController.hasStarted;
            return shouldCharge;
        }
    }

    // TODO: base storm state and inheritance and defined levels instead of this big stupid fuck storm state
    public class Storm : BaseWeatherState
    {
        private static float baseDuration = 80f;
        private static float durationVariance = 0.4f;

        // Storm elite spawning values
        private static int bossEliteLevel = 3;
        private static int eliteLevel = 2;
        private static float eliteChancePerExtraLevelCoefficient = 2f;
        private static float eliteChancePerSecond = 2.5f;
        private static float baseEliteChance = 0f;
        private static float maxEliteChanceInterval = 24f;
        private static float minEliteChanceInterval = 4f;
        private float currentEliteChance;
        private float eliteChanceStopwatch;
        private float eliteChanceInterval;

        // Ball lightning spawning values
        private static float ballSpawnAttemptInterval = 3f;
        private static float ballChancePerSecond = 0.5f;
        private static float ballMinSpawnDistance = 10f;
        private static float ballMaxSpawnDistance = 120f;
        private float ballSpawnStopwatch;

        // Monster wave values

        public float lerpDuration;
        public int stormLevel;
        private bool isPermanent;

        private float duration;

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(this.stormLevel);
            writer.Write(this.lerpDuration);
            writer.Write(duration);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            this.stormLevel = reader.ReadInt32();
            this.lerpDuration = reader.ReadSingle();
            duration = reader.ReadSingle();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            stormController.SetStormLevel(stormLevel); // ???????????????????????????? dude
            this.stormController.StartEffectIntensityLerp(stormLevel, lerpDuration);
            stormController.StartBarAnimation(stormLevel, lerpDuration);
            stormController.SetBarActive(true); // in case we skipped calm

            isPermanent = stormController.IsPermanent && this.stormLevel >= stormController.MaxStormLevel;

            eliteChanceInterval = UnityEngine.Random.Range(minEliteChanceInterval, maxEliteChanceInterval);

            SetHooks();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (stormController)
            {
                stormController.progressInCurrentLevel = fixedAge / duration;
            }


            if (!NetworkServer.active) return;

            // Update elite chance
            eliteChanceStopwatch -= Time.fixedDeltaTime;
            if (eliteChanceStopwatch <= 0)
            {
                // add elite chance according to current interval
                currentEliteChance += eliteChancePerSecond * eliteChanceInterval;
                if (currentEliteChance > 100) currentEliteChance = 100;

                // start new interval
                eliteChanceInterval = UnityEngine.Random.Range(minEliteChanceInterval, maxEliteChanceInterval);
                eliteChanceStopwatch += eliteChanceInterval;
            }

            // Update ball chance
            // TODO: guaranteed ball lightning spawns during upgraded tp event
            ballSpawnStopwatch -= Time.fixedDeltaTime;
            if (ballSpawnStopwatch <= 0)
            {
                ballSpawnStopwatch += ballSpawnAttemptInterval;

                float percentChance = ballChancePerSecond * ballSpawnAttemptInterval * 0.01f;
                if (stormController.ballRng.nextNormalizedFloat >= percentChance)
                {
                    SpawnBall();
                }
            }

            if (!isPermanent && fixedAge > duration && ShouldCharge())
            {
                if (stormLevel == stormController.MaxStormLevel && !stormController.IsPermanent)
                {
                    outer.SetNextState(new Calm());
                    return;
                }

                outer.SetNextState(new Storm { stormLevel = stormLevel + 1, lerpDuration = 8f });
                return;
            }
        }
        private bool ShouldCharge()
        {
            bool shouldCharge = !TeleporterInteraction.instance;
            shouldCharge |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle;
            return shouldCharge;
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);

            if (nextState is Storm stormState)
            {
                stormState.currentEliteChance = this.currentEliteChance;
            }
        }

        public override void OnExit()
        {
            UnsetHooks();

            base.OnExit();
        }
        private void SpawnBall()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            Transform spawnTarget = null;
            DirectorPlacementRule.PlacementMode placementMode = DirectorPlacementRule.PlacementMode.Random;

            // pick random player, then spawn randomly in a radius around it
            List<GameObject> playerBodyObjects = new List<GameObject>();
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController._instances)
            {
                if (player.master.hasBody)
                {
                    playerBodyObjects.Add(player.master.GetBodyObject());
                }
            }
            var playerBodyObject = stormController.ballRng.NextElementUniform(playerBodyObjects);
            if (playerBodyObject)
            {
                spawnTarget = playerBodyObject.transform;
                placementMode = DirectorPlacementRule.PlacementMode.Approximate;
            }

            var ballSpawnCard = SS2Assets.LoadAsset<SpawnCard>("iscBallLightningPickup", SS2Bundle.Events);
            DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(ballSpawnCard, new DirectorPlacementRule
            {
                minDistance = ballMinSpawnDistance,
                maxDistance = ballMaxSpawnDistance,
                placementMode = placementMode,
                spawnOnTarget = spawnTarget,
            }, stormController.ballRng));
        }


        // hooks go in the uggo corner
        #region Event Hooks

        private void SetHooks()
        {
            if (NetworkServer.active)
            {
                float variance = baseDuration * durationVariance;
                this.duration = stormController.timeRng.RangeFloat(baseDuration - variance, baseDuration + variance);

                // TODO: probably put these events into stormcontroller?
                CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
                if (bossDirector && stormLevel >= bossEliteLevel)
                {
                    if (TeleporterUpgradeController.instance) TeleporterUpgradeController.instance.UpgradeStorm(true);
                    BossGroup.onBossGroupDefeatedServer += OnBossGroupDefeatedServer;
                    bossDirector.onSpawnedServer.AddListener(ModifySpawnedBoss);
                }

                foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                {
                    if (combatDirector != bossDirector)
                        combatDirector.onSpawnedServer.AddListener(ModifySpawnedMasters);
                }

                CharacterBody.onBodyStartGlobal += BuffEnemy;
                SetBuffCountGlobal(stormLevel);
            }

            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterChargedGlobal;
        }

        private void UnsetHooks()
        {
            CharacterBody.onBodyStartGlobal -= BuffEnemy;
            TeleporterInteraction.onTeleporterChargedGlobal -= OnTeleporterChargedGlobal;
            if (NetworkServer.active)
            {
                // removelistener makes it so we cant add it back in the next state's onenter. (wtf?????)
                CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
                if (bossDirector && stormLevel >= bossEliteLevel)
                {
                    BossGroup.onBossGroupDefeatedServer -= OnBossGroupDefeatedServer;
                    bossDirector.onSpawnedServer.RemoveListener(ModifySpawnedBoss);
                }
                if (!stormController.IsPermanent)
                {
                    foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                    {
                        if (combatDirector != bossDirector)
                            combatDirector.onSpawnedServer.RemoveListener(ModifySpawnedMasters);
                    }
                }
            }
        }
        private void OnTeleporterChargedGlobal(TeleporterInteraction _)
        {
            this.outer.SetNextState(new Calm());
        }

        private void ModifySpawnedMasters(GameObject masterObject)
        {
            int extraLevels = this.stormLevel - eliteLevel;
            float chance = baseEliteChance + currentEliteChance * (1 + extraLevels * eliteChancePerExtraLevelCoefficient);
            if (Util.CheckRoll(chance))
            {
                currentEliteChance /= 2f;
                CreateStormElite(masterObject);
            }
        }
        private void ModifySpawnedBoss(GameObject masterObject)
        {
            CreateStormElite(masterObject);
            GameObject bodyObject = masterObject.GetComponent<CharacterMaster>().GetBodyObject();
            if (bodyObject)
            {
                bodyObject.AddComponent<OnBossKilled>();
            }
        }
        private void CreateStormElite(GameObject masterObject)
        {
            CharacterMaster master = masterObject.GetComponent<CharacterMaster>();
            if (master.inventory.currentEquipmentIndex == SS2Content.Equipments.AffixEmpyrean.equipmentIndex) return;
            master.inventory.GiveItem(SS2Content.Items.AffixStorm);
            GameObject bodyObject = master.GetBodyObject();
            if (bodyObject)
            {
                EntityStateMachine bodyMachine = EntityStateMachine.FindByCustomName(bodyObject, "Body");
                bodyMachine.initialStateType = new SerializableEntityStateType(typeof(AffixStorm.SpawnState));
            }
        }

        private void BuffEnemy(CharacterBody body)
        {
            if (!NetworkServer.active)
                return;
            var team = body.teamComponent.teamIndex;
            var mask = TeamMask.GetEnemyTeams(TeamIndex.Player);
            mask.RemoveTeam(TeamIndex.Neutral);
            if (mask.HasTeam(team) && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
            {
                body.SetBuffCount(SS2Content.Buffs.BuffStorm.buffIndex, stormLevel);
            }
        }

        private void OnBossGroupDefeatedServer(BossGroup bossGroup)
        {
            if (bossGroup == TeleporterInteraction.instance.bossGroup && Run.instance.participatingPlayerCount > 0)
            {
                int playerCount = Run.instance.participatingPlayerCount;
                float angle = 360f / (float)playerCount;
                Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                PickupIndex drop = PickupCatalog.FindPickupIndex(SS2Content.Items.ShardStorm.itemIndex);
                int i = 0;
                while (i < playerCount)
                {
                    PickupDropletController.CreatePickupDroplet(drop, bossGroup.dropPosition.position, vector);
                    i++;
                    vector = rotation * vector;
                }
            }
        }

        // am i just stupid? where the fuck is the event
        private class OnBossKilled : MonoBehaviour, IOnKilledServerReceiver
        {
            public void OnKilledServer(DamageReport damageReport)
            {
                PickupIndex pickupIndex = StormController.dropTable.GenerateDrop(StormController.instance.treasureRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
        }
        #endregion

    }

}
