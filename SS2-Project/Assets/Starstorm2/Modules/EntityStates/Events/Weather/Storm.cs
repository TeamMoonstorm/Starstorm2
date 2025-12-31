#region OLD
using SS2.Components;
using RoR2;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using SS2;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace EntityStates.Events
{
    // being lazy. should be StormX : Storm and use entitystateconfigs
    public class Storm : GenericWeatherState
    {
        private static float chargeInterval = 10f;
        private static float chargeVariance = 0.66f;
        private static float chargeFromKill = 0.5f;
        private static float effectLerpDuration = 5f;
        private static SerializableEntityStateType textState = new SerializableEntityStateType(typeof(StormController.EtherealFadeIn));
        private static SerializableEntityStateType textState2 = new SerializableEntityStateType(typeof(StormController.EtherealBlinkIn)); // fuck my life
        private static Color textColor = Color.gray;
        private static int bossEliteLevel = 4;
        private static int eliteLevel = 3;
        private static float eliteChancePerExtraLevelCoefficient = 2f;
        private static float eliteChancePerSecond = 2.5f;
        private static float baseEliteChance = 10f;

        private float charge;
        private float chargeStopwatch = 10f; // longer stopwatch means more variance
        private bool isPermanent;
        private float eliteChance;
        private float eliteChanceStopwatch;
        private float eliteChanceTimer;
        private float maxEliteChanceInterval = 24f;
        private float minEliteChanceInterval = 4f;

        public float lerpDuration;
        public int stormLevel;
        public bool instantStorm;
        private bool oldStorm;

        private static float oldstormelitemutlifewafa = 0.67f;
        private float oldStormMinDuration = 120;
        private float oldStormMaxDuration = 180;
        private float oldStormDuration;
        private UnityAction<GameObject> modifyMonsters;
        private UnityAction<GameObject> modifyBoss;
        public override void OnEnter()
        {
            base.OnEnter();




            oldStorm = !SS2.Storm.ReworkedStorm;

            if (true)
            {
                if (oldStorm)
                {
                    oldStormDuration = UnityEngine.Random.Range(oldStormMinDuration, oldStormMaxDuration);
                    GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
                    {
                        eventToken = GETDUMBASSTOKENDELETELATER() + "_START",
                        eventColor = textColor,
                        textDuration = 6,
                    };
                    GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);
                }
                else
                {
                    GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
                    {
                        eventToken = "ermmmm..... storm " + stormLevel,
                        eventColor = textColor,
                        textDuration = 6,
                    };
                    GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);
                }
            }

            this.stormController.StartLerp(stormLevel, lerpDuration);


            isPermanent = stormController.IsPermanent && this.stormLevel >= stormController.MaxStormLevel;

            if (NetworkServer.active)
            {
                CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
                if (bossDirector && stormLevel >= bossEliteLevel)
                {
                    if (TeleporterUpgradeController.instance) TeleporterUpgradeController.instance.UpgradeStorm(true);

                    bossDirector.onSpawnedServer.AddListener(modifyBoss = new UnityAction<GameObject>(ModifySpawnedBoss));
                }


                foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                {
                    if (combatDirector != bossDirector)
                        combatDirector.onSpawnedServer.AddListener(modifyMonsters = new UnityAction<GameObject>(ModifySpawnedMasters));
                }

                CharacterBody.onBodyStartGlobal += BuffEnemy;
                var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar)).Concat(TeamComponent.GetTeamMembers(TeamIndex.Void));
                foreach (var teamMember in enemies)
                {
                    BuffEnemy(teamMember.body);
                }
            }

            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterChargedGlobal;
        }

        private void OnTeleporterChargedGlobal(TeleporterInteraction _)
        {
            this.outer.SetNextState(new Calm());
        }

        private void ModifySpawnedMasters(GameObject masterObject)
        {
            int extraLevels = this.stormLevel - eliteLevel;
            float chance = eliteChance * (1 + extraLevels * eliteChancePerExtraLevelCoefficient);
            if (oldStorm) chance *= oldstormelitemutlifewafa;
            if (Util.CheckRoll(chance))
            {
                eliteChance /= 2f;
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
            int buffCount = body.GetBuffCount(SS2Content.Buffs.BuffStorm);
            if (TeamMask.GetEnemyTeams(TeamIndex.Player).HasTeam(team) && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
            {
                int buffsToGrant = stormLevel - buffCount;
                for (int i = 0; i < buffsToGrant; i++)
                    body.AddBuff(SS2Content.Buffs.BuffStorm);
            }
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active) return;

            // im stupid or what
            this.eliteChanceStopwatch -= Time.fixedDeltaTime;
            eliteChanceTimer += Time.fixedDeltaTime;
            if (eliteChanceStopwatch <= 0)
            {
                eliteChanceStopwatch += UnityEngine.Random.Range(minEliteChanceInterval, maxEliteChanceInterval);
                eliteChance += eliteChancePerSecond * eliteChanceTimer;
                if (eliteChance > 100) eliteChance = 100;
                eliteChanceTimer = 0;
            }
            if (oldStorm)
            {
                if (base.fixedAge >= oldStormDuration)
                {
                    GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
                    {
                        eventToken = GETDUMBASSTOKENDELETELATER() + "_END",
                        eventColor = textColor,
                        textDuration = 6,
                    };
                    GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);
                    outer.SetNextState(new Calm { thing = 3f });

                    var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar)).Concat(TeamComponent.GetTeamMembers(TeamIndex.Void));
                    foreach (var teamMember in enemies)
                    {
                        int buffCount = teamMember.body.GetBuffCount(SS2Content.Buffs.BuffStorm);
                        for (int i = 0; i < buffCount; i++)
                            teamMember.body.RemoveBuff(SS2Content.Buffs.BuffStorm);

                    }
                }
            }
            else
            {
                this.chargeStopwatch -= Time.fixedDeltaTime;
                if (this.chargeStopwatch <= 0 && ShouldCharge())
                {
                    this.chargeStopwatch = chargeInterval;
                    float charge = CalculateCharge(chargeInterval);
                    this.charge += charge;
                    this.stormController.AddCharge(charge);
                }

                if (!isPermanent && charge >= 100f)
                {
                    if (stormLevel == stormController.MaxStormLevel && !stormController.IsPermanent)
                    {
                        outer.SetNextState(new Calm());
                        var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar)).Concat(TeamComponent.GetTeamMembers(TeamIndex.Void));
                        foreach (var teamMember in enemies)
                        {
                            int buffCount = teamMember.body.GetBuffCount(SS2Content.Buffs.BuffStorm);
                            for (int i = 0; i < buffCount; i++)
                                teamMember.body.RemoveBuff(SS2Content.Buffs.BuffStorm);

                        }
                        GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
                        {
                            eventToken = "ermmmm..... bye storm",
                            eventColor = textColor,
                            textDuration = 6,
                        };
                        GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);
                        return;
                    }

                    this.stormController.OnStormLevelCompleted();
                    outer.SetNextState(new Storm { stormLevel = stormLevel + 1, lerpDuration = 8f });
                    return;
                }
            }

        }

        private string GETDUMBASSTOKENDELETELATER()
        {
            string fuk;
            switch (R2API.DirectorAPI.GetStageEnumFromSceneDef(Stage.instance.sceneDef))
            {
                case R2API.DirectorAPI.Stage.RallypointDelta:
                case R2API.DirectorAPI.Stage.SiphonedForest:
                    fuk = "SS2_EVENT_BLIZZARD";
                    break;
                case R2API.DirectorAPI.Stage.AbyssalDepths:
                case R2API.DirectorAPI.Stage.ScorchedAcres:
                    fuk = "SS2_EVENT_ASHSTORM";
                    break;
                case R2API.DirectorAPI.Stage.AbandonedAqueduct:
                    fuk = "SS2_EVENT_SANDSTORM";
                    break;
                default:
                    fuk = "SS2_EVENT_THUNDERSTORM";
                    break;
            }
            return fuk;
        }
        private bool ShouldCharge()
        {
            bool shouldCharge = !TeleporterInteraction.instance;
            shouldCharge |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle;
            return shouldCharge;
        }

        private float CalculateCharge(float deltaTime)
        {
            float timeToCharge = 60f;
            float creditsPerSecond = 100f / timeToCharge;
            float variance = chargeVariance * creditsPerSecond;

            float charge = StormController.chargeRng.RangeFloat(creditsPerSecond - variance, creditsPerSecond + variance) * deltaTime;
            return charge;

        }

        public override void OnExit()
        {
            base.OnExit();

            CharacterBody.onBodyStartGlobal -= BuffEnemy;
            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterChargedGlobal;
            if (NetworkServer.active)
            {
                // removelistener makes it so we cant add it back in the next state's onenter. (wtf?????)
                CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
                if (bossDirector && stormLevel >= 4)
                {
                    bossDirector.onSpawnedServer.RemoveListener(modifyBoss);
                }
                if (oldStorm || !stormController.IsPermanent)
                {
                    foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                    {
                        if (combatDirector != bossDirector)
                            combatDirector.onSpawnedServer.RemoveListener(modifyMonsters);
                    }
                }
            }


        }



        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(this.stormLevel);
            writer.Write(this.lerpDuration);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            this.stormLevel = reader.ReadInt32();
            this.lerpDuration = reader.ReadSingle();
        }

        // am i just stupid? where the fuck is the event
        private class OnBossKilled : MonoBehaviour, IOnKilledServerReceiver
        {
            public void OnKilledServer(DamageReport damageReport)
            {
                UniquePickup pickup = StormController.dropTable.GeneratePickup(StormController.instance.treasureRng);
                if (pickup.pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
                    {
                        pickup = pickup,
                    },
                    damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
        }
    }

}
#endregion

#region NEW
//using SS2.Components;
//using RoR2;
//using UnityEngine;
//using MSU;
//using UnityEngine.Networking;
//using SS2;
//using RoR2.UI;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine.Events;
//namespace EntityStates.Events
//{
//    // being lazy. should be StormX : Storm and use entitystateconfigs
//    public class Storm : GenericWeatherState
//    {
//        private static float baseDuration = 60f;
//        private static float durationVariance = 0.4f;
//        private static float effectLerpDuration = 5f;
//        private static SerializableEntityStateType textState = new SerializableEntityStateType(typeof(StormController.EtherealFadeIn));
//        private static SerializableEntityStateType textState2 = new SerializableEntityStateType(typeof(StormController.EtherealBlinkIn)); // fuck my life
//        private static Color textColor = Color.gray;
//        private static int bossEliteLevel = 4;
//        private static int eliteLevel = 3;
//        private static float eliteChancePerExtraLevelCoefficient = 2f;
//        private static float eliteChancePerSecond = 2.5f;
//        private static float baseEliteChance = 10f;
//        private bool isPermanent;
//        private float eliteChance;
//        private float eliteChanceStopwatch;
//        private float eliteChanceTimer;
//        private float maxEliteChanceInterval = 24f;
//        private float minEliteChanceInterval = 4f;

//        public float lerpDuration;
//        public int stormLevel;

//        private float duration;
//        public override void OnEnter()
//        {
//            base.OnEnter();

//            GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
//            {
//                eventToken = "ermmmm..... storm " + stormLevel,
//                eventColor = textColor,
//                textDuration = 6,
//            };
//            GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);
//            this.stormController.StartLerp(stormLevel, lerpDuration);
//            isPermanent = stormController.IsPermanent && this.stormLevel >= stormController.MaxStormLevel;

//            if (NetworkServer.active)
//            {
//                float variance = baseDuration * durationVariance;
//                this.duration = stormController.timeRng.RangeFloat(baseDuration - variance, baseDuration + variance);

//                CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
//                if (bossDirector && stormLevel >= bossEliteLevel)
//                {
//                    if (TeleporterUpgradeController.instance) TeleporterUpgradeController.instance.UpgradeStorm(true);
//                    BossGroup.onBossGroupDefeatedServer += OnBossGroupDefeatedServer;
//                    bossDirector.onSpawnedServer.AddListener(ModifySpawnedBoss);
//                }

//                foreach (CombatDirector combatDirector in CombatDirector.instancesList)
//                {
//                    if (combatDirector != bossDirector)
//                        combatDirector.onSpawnedServer.AddListener(ModifySpawnedMasters);
//                }

//                CharacterBody.onBodyStartGlobal += BuffEnemy;
//                var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar)).Concat(TeamComponent.GetTeamMembers(TeamIndex.Void));
//                foreach (var teamMember in enemies)
//                {
//                    BuffEnemy(teamMember.body);
//                }
//            }

//            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterChargedGlobal;
//        }

//        private void OnBossGroupDefeatedServer(BossGroup bossGroup)
//        {
//            if (bossGroup == TeleporterInteraction.instance.bossGroup && Run.instance.participatingPlayerCount > 0)
//            {
//                int playerCount = Run.instance.participatingPlayerCount;
//                float angle = 360f / (float)playerCount;
//                Vector3 vector = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
//                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
//                PickupIndex drop = PickupCatalog.FindPickupIndex(SS2Content.Items.ShardStorm.itemIndex);
//                int i = 0;
//                while (i < playerCount)
//                {
//                    PickupDropletController.CreatePickupDroplet(drop, bossGroup.dropPosition.position, vector);
//                    i++;
//                    vector = rotation * vector;
//                }
//            }
//        }

//        private void OnTeleporterChargedGlobal(TeleporterInteraction _)
//        {
//            this.outer.SetNextState(new Calm());
//        }

//        private void ModifySpawnedMasters(GameObject masterObject)
//        {
//            int extraLevels = this.stormLevel - eliteLevel;
//            float chance = eliteChance * (1 + extraLevels * eliteChancePerExtraLevelCoefficient);
//            if (Util.CheckRoll(chance))
//            {
//                eliteChance /= 2f;
//                CreateStormElite(masterObject);
//            }
//        }
//        private void ModifySpawnedBoss(GameObject masterObject)
//        {
//            CreateStormElite(masterObject);
//            GameObject bodyObject = masterObject.GetComponent<CharacterMaster>().GetBodyObject();
//            if (bodyObject)
//            {
//                bodyObject.AddComponent<OnBossKilled>();
//            }
//        }
//        private void CreateStormElite(GameObject masterObject)
//        {
//            CharacterMaster master = masterObject.GetComponent<CharacterMaster>();
//            if (master.inventory.currentEquipmentIndex == SS2Content.Equipments.AffixEmpyrean.equipmentIndex) return;
//            master.inventory.GiveItem(SS2Content.Items.AffixStorm);
//            master.inventory.GiveItem(SS2Content.Items.MaxHealthPerMinute, 2 * (stormLevel - eliteLevel)); // lvl 3 = 2, lvl4 = 4
//            GameObject bodyObject = master.GetBodyObject();
//            if (bodyObject)
//            {
//                EntityStateMachine bodyMachine = EntityStateMachine.FindByCustomName(bodyObject, "Body");
//                bodyMachine.initialStateType = new SerializableEntityStateType(typeof(AffixStorm.SpawnState));
//            }
//        }

//        private void BuffEnemy(CharacterBody body)
//        {
//            if (!NetworkServer.active)
//                return;
//            var team = body.teamComponent.teamIndex;
//            if (TeamMask.GetEnemyTeams(TeamIndex.Player).HasTeam(team) && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
//            {
//                body.SetBuffCount(SS2Content.Buffs.BuffStorm.buffIndex, stormLevel);
//            }
//        }

//        public override void FixedUpdate()
//        {
//            base.FixedUpdate();
//            if (!NetworkServer.active) return;

//            // im stupid or what
//            this.eliteChanceStopwatch -= Time.fixedDeltaTime;
//            eliteChanceTimer += Time.fixedDeltaTime;
//            if (eliteChanceStopwatch <= 0)
//            {
//                eliteChanceStopwatch += UnityEngine.Random.Range(minEliteChanceInterval, maxEliteChanceInterval);
//                eliteChance += eliteChancePerSecond * eliteChanceTimer;
//                if (eliteChance > 100) eliteChance = 100;
//                eliteChanceTimer = 0;
//            }
//            if (!isPermanent && base.fixedAge > duration && ShouldCharge())
//            {
//                if (stormLevel == stormController.MaxStormLevel && !stormController.IsPermanent)
//                {
//                    outer.SetNextState(new Calm());
//                    if (TeleporterUpgradeController.instance) TeleporterUpgradeController.instance.UpgradeStorm(false);
//                    GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
//                    {
//                        eventToken = "ermmmm..... bye storm",
//                        eventColor = textColor,
//                        textDuration = 6,
//                    };
//                    GameplayEventTextController.instance.EnqueueNewTextRequest(request, false);
//                    return;
//                }

//                this.stormController.OnStormLevelCompleted();
//                outer.SetNextState(new Storm { stormLevel = stormLevel + 1, lerpDuration = 8f });
//                return;
//            }
//        }

//        private bool ShouldCharge()
//        {
//            bool shouldCharge = !TeleporterInteraction.instance;
//            shouldCharge |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle;
//            return shouldCharge;
//        }

//        public override void OnExit()
//        {
//            base.OnExit();

//            CharacterBody.onBodyStartGlobal -= BuffEnemy;
//            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterChargedGlobal;
//            if (NetworkServer.active)
//            {
//                // removelistener makes it so we cant add it back in the next state's onenter. (wtf?????)
//                CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
//                if (bossDirector && stormLevel >= 4)
//                {
//                    BossGroup.onBossGroupDefeatedServer -= OnBossGroupDefeatedServer;
//                    bossDirector.onSpawnedServer.RemoveListener(ModifySpawnedBoss);
//                }
//                if (!stormController.IsPermanent)
//                {
//                    foreach (CombatDirector combatDirector in CombatDirector.instancesList)
//                    {
//                        if (combatDirector != bossDirector)
//                            combatDirector.onSpawnedServer.RemoveListener(ModifySpawnedMasters);
//                    }
//                }

//                if (this.outer.nextState is Calm) // un buff
//                {
//                    var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar)).Concat(TeamComponent.GetTeamMembers(TeamIndex.Void));
//                    foreach (var teamMember in enemies)
//                    {
//                        teamMember.body.SetBuffCount(SS2Content.Buffs.BuffStorm.buffIndex, 0);
//                    }
//                }
//            }
//        }
//        public override void OnSerialize(NetworkWriter writer)
//        {
//            writer.Write(this.stormLevel);
//            writer.Write(this.lerpDuration);
//        }
//        public override void OnDeserialize(NetworkReader reader)
//        {
//            this.stormLevel = reader.ReadInt32();
//            this.lerpDuration = reader.ReadSingle();
//        }

//        // am i just stupid? where the fuck is the event
//        private class OnBossKilled : MonoBehaviour, IOnKilledServerReceiver
//        {
//            public void OnKilledServer(DamageReport damageReport)
//            {
//                PickupIndex pickupIndex = StormController.dropTable.GenerateDrop(StormController.instance.treasureRng);
//                if (pickupIndex != PickupIndex.none)
//                {
//                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
//                }
//            }
//        }
//    }

//}
#endregion