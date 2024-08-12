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
        private static float eliteChancePerSecond = 3f;
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

        private UnityAction<GameObject> modifyMonsters;
        private UnityAction<GameObject> modifyBoss;
        public override void OnEnter()
        {
            base.OnEnter();


            bool skip = stormController.AttemptSkip();
            GameplayEventTextController.EventTextRequest request = new GameplayEventTextController.EventTextRequest
            {
                eventToken = "ermmmm..... storm " + stormLevel,
                eventColor = textColor,
                textDuration = 6,
            };
            if (skip)
            {              
                this.stormLevel++;             
                stormController.OnStormLevelCompleted(); /////////////////////////////////////////////////////////////////// lmao
                request.eventToken = "ermmmm..... storm " + stormLevel;
                request.customTextState = instantStorm ? textState2 : textState;
                request.textDuration = 2;
            }
            GameplayEventTextController.Instance.EnqueueNewTextRequest(request);
            
            if(instantStorm)
                this.stormController.StartLerp(stormLevel, 0);
            else
                this.stormController.StartLerp(stormLevel, lerpDuration);


            isPermanent = stormController.IsPermanent && this.stormLevel == stormController.MaxStormLevel;

            if (NetworkServer.active)
            {
                CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
                if (bossDirector && stormLevel >= bossEliteLevel)
                {
                    bossDirector.onSpawnedServer.AddListener(modifyBoss = new UnityAction<GameObject>(ModifySpawnedBoss));
                }


                foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                {
                    if (combatDirector != bossDirector)
                        combatDirector.onSpawnedServer.AddListener(modifyMonsters = new UnityAction<GameObject>(ModifySpawnedMasters));
                }

            }

            CharacterBody.onBodyStartGlobal += BuffEnemy;
            var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar)).Concat(TeamComponent.GetTeamMembers(TeamIndex.Void));
            foreach (var teamMember in enemies)
            {
                BuffEnemy(teamMember.body);
            }
        }

        private void ModifySpawnedMasters(GameObject masterObject)
        {
            int extraLevels = this.stormLevel - eliteLevel + 1;
            if (Util.CheckRoll(eliteChance * extraLevels * eliteChancePerExtraLevelCoefficient))
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
            if(eliteChanceStopwatch <= 0)
            {               
                eliteChanceStopwatch += UnityEngine.Random.Range(minEliteChanceInterval, maxEliteChanceInterval);
                eliteChance += eliteChancePerSecond * eliteChanceTimer;
                eliteChanceTimer = 0;
            }
            this.chargeStopwatch -= Time.fixedDeltaTime;
            if(this.chargeStopwatch <= 0 && ShouldCharge())
            {
                this.chargeStopwatch = chargeInterval; 
                float charge = CalculateCharge(chargeInterval);
                this.charge += charge;
                this.stormController.AddCharge(charge);
            }

            if (!isPermanent && charge >= 100f)
            {
                this.stormController.OnStormLevelCompleted();
                outer.SetNextState(new Storm { stormLevel = stormLevel + 1, lerpDuration = 8f });
                return;
            }               
        }

        private bool ShouldCharge()
        {
            bool shouldCharge = !TeleporterInteraction.instance;
            shouldCharge |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle;
            shouldCharge &= this.stormLevel < this.stormController.MaxStormLevel;
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
            SS2Log.Info($"Storm level finished in {fixedAge} seconds.");

            CharacterBody.onBodyStartGlobal -= BuffEnemy;

            CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
            if (bossDirector && stormLevel >= 4)
            {
                bossDirector.onSpawnedServer.RemoveListener(modifyBoss);
            }
            foreach (CombatDirector combatDirector in CombatDirector.instancesList)
            {
                if (combatDirector != bossDirector)
                    combatDirector.onSpawnedServer.RemoveListener(modifyMonsters);
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
        // TODO: souls curios idk whenever the fuck our modelers stop being depressed sacks of shit
        private class OnBossKilled : MonoBehaviour, IOnKilledServerReceiver
        {
            public void OnKilledServer(DamageReport damageReport)
            {
                PickupIndex pickupIndex = StormController.dropTable.GenerateDrop(StormController.treasureRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
        }
    }

}