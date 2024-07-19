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
    public class Storm : GenericWeatherState
    {
        private static float chargeInterval = 10f;
        private static float chargeVariance = 0.66f;
        private static float chargeFromKill = 0.5f;
        private static float effectLerpDuration = 5f;
        
        public Storm(int stormLevel, float lerpDuration)
        {
            this.stormLevel = stormLevel;
            this.lerpDuration = lerpDuration;
        }

        private float charge;
        private float chargeStopwatch = 10f;

        private float lerpDuration;
        private int stormLevel;
        private int maxStormLevel;

        private float chargeFromKills;
        private float chargeFromTime;
        public override void OnEnter()
        {
            base.OnEnter();

            GameplayEventTextController.Instance.EnqueueNewTextRequest(new GameplayEventTextController.EventTextRequest
            {
                eventToken = "ermmmm..... storm " + stormLevel,
                eventColor = Color.gray,
                textDuration = 6,
            });
            this.stormController.StartLerp(stormLevel, lerpDuration);
            maxStormLevel = Mathf.FloorToInt(DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue) + 2; // drizzle 3, monsoon/typhoon 5
            if(NetworkServer.active)
            {
                GlobalEventManager.onCharacterDeathGlobal += AddCharge;
                CharacterBody.onBodyStartGlobal += BuffEnemy;

                var enemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Concat(TeamComponent.GetTeamMembers(TeamIndex.Lunar)).Concat(TeamComponent.GetTeamMembers(TeamIndex.Void));
                foreach (var teamMember in enemies)
                {
                    BuffEnemy(teamMember.body);
                }

                CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
                if (bossDirector && stormLevel >= 5)
                {                 
                    bossDirector.onSpawnedServer.AddListener(new UnityAction<GameObject>(ModifySpawnedBoss));
                }
                    

                foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                {
                    if(combatDirector != bossDirector)
                        combatDirector.onSpawnedServer.AddListener(new UnityAction<GameObject>(ModifySpawnedMasters));
                }
                
            }
            
            
        }

        private static float SPAWNCHANCETEMPJUSTFORTESTINGIPROMISE = 10f;
        // TODO: move elite spawning to stormcontroller, use credits
        // idk why this is in the entitystates tbh
        private void ModifySpawnedMasters(GameObject masterObject)
        {
            if(Util.CheckRoll(SPAWNCHANCETEMPJUSTFORTESTINGIPROMISE * (stormLevel - 3)))
            {
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

        // TODO: stronger enemies give more charge(?), move charge to stormcontroller(?)
        private void AddCharge(DamageReport damageReport)
        {
            CharacterBody body = damageReport.victimBody;
            if(ShouldCharge() && TeamMask.GetEnemyTeams(TeamIndex.Player).HasTeam(damageReport.victimTeamIndex))//body.HasBuff(SS2Content.Buffs.BuffStorm) && damageReport.attackerTeamIndex == TeamIndex.Player)
            {
                float charge = chargeFromKill;
                float variance = chargeVariance * charge;

                float charge1 = StormController.mobChargeRng.RangeFloat(charge - variance, charge + variance);
                this.chargeFromKills += charge1;
                this.charge += charge1;
                this.stormController.AddCharge(charge1);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active) return;

            this.chargeStopwatch -= Time.fixedDeltaTime;
            if(this.chargeStopwatch <= 0 && ShouldCharge())
            {
                this.chargeStopwatch = chargeInterval; 
                float charge = CalculateCharge(chargeInterval);
                this.charge += charge;
                this.stormController.AddCharge(charge);
            }

            if (stormLevel < maxStormLevel && charge >= 100f)
            {
                outer.SetNextState(new Storm(stormLevel: stormLevel + 1, lerpDuration: 15f));
                return;
            }
                
        }

        private bool ShouldCharge()
        {
            bool shouldCharge = !TeleporterInteraction.instance;
            shouldCharge |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle && stormLevel < maxStormLevel;
            return shouldCharge;
        }

        private float CalculateCharge(float deltaTime)
        {
            float timeToCharge = 60f;
            float creditsPerSecond = 100f / timeToCharge;
            float variance = chargeVariance * creditsPerSecond;

            float charge = StormController.chargeRng.RangeFloat(creditsPerSecond - variance, creditsPerSecond + variance) * deltaTime;
            this.chargeFromTime += charge;
            return charge;

        }

        public override void OnExit()
        {
            base.OnExit();
            SS2Log.Info($"Storm level finished in {fixedAge} seconds.");
            SS2Log.Info($"Charge from kills = {chargeFromKills}");
            SS2Log.Info($"Charge form time = {chargeFromTime}");

            GlobalEventManager.onCharacterDeathGlobal -= AddCharge;
            CharacterBody.onBodyStartGlobal -= BuffEnemy;

            CombatDirector bossDirector = TeleporterInteraction.instance?.bossDirector;
            if (bossDirector && stormLevel >= 5)
            {
                bossDirector.onSpawnedServer.RemoveListener(new UnityEngine.Events.UnityAction<GameObject>(ModifySpawnedBoss));
            }
            foreach (CombatDirector combatDirector in CombatDirector.instancesList)
            {
                if (combatDirector != bossDirector)
                    combatDirector.onSpawnedServer.RemoveListener(new UnityEngine.Events.UnityAction<GameObject>(ModifySpawnedMasters));
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
                for(int i = 0; i < buffsToGrant; i++)
                    body.AddBuff(SS2Content.Buffs.BuffStorm);
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(this.stormLevel);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            this.stormLevel = reader.ReadInt32();
        }

        // am i just stupid? where the fuck is the event
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