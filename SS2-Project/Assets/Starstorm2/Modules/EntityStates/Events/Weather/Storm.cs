using SS2.Components;
using RoR2;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using SS2;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;

namespace EntityStates.Events
{
    public class Storm : GenericWeatherState
    {
        private static float chargeInterval = 10f;
        private static float chargeVariance = 0.66f;
        private static float chargeFromKill = 0.5f;
        private static float effectLerpDuration = 5f;
        private static int maxStormLevel = 5; // by difficulty?
        public Storm(int stormLevel, float lerpDuration)
        {
            this.stormLevel = stormLevel;
            this.lerpDuration = lerpDuration;
        }

        private float charge;
        private Xoroshiro128Plus rng;
        private Xoroshiro128Plus mobRng;
        private float chargeStopwatch = 10f;

        private float lerpDuration;
        private int stormLevel;

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

            if(NetworkServer.active)
            {
                rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
                mobRng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);

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
                    bossDirector.onSpawnedServer.AddListener(new UnityEngine.Events.UnityAction<GameObject>(ModifySpawnedBoss));
                }
                    

                foreach (CombatDirector combatDirector in CombatDirector.instancesList)
                {
                    if(combatDirector != bossDirector)
                        combatDirector.onSpawnedServer.AddListener(new UnityEngine.Events.UnityAction<GameObject>(ModifySpawnedMasters));
                }
                
            }
            
            
        }

        private static float SPAWNCHANCETEMPJUSTFORTESTINGIPROMISE = 10f;
        // TODO: move elite spawning to stormcontroller, use credits
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

        // TODO: stronger enemies give more charge(?), move charge to stormcontroller, stop charge gain during teleporter
        private void AddCharge(DamageReport damageReport)
        {
            CharacterBody body = damageReport.victimBody;
            if(TeamMask.GetEnemyTeams(TeamIndex.Player).HasTeam(damageReport.victimTeamIndex))//body.HasBuff(SS2Content.Buffs.BuffStorm) && damageReport.attackerTeamIndex == TeamIndex.Player)
            {
                float charge = chargeFromKill;
                float variance = chargeVariance * charge;

                float charge1 = mobRng.RangeFloat(charge - variance, charge + variance);
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
            if(this.chargeStopwatch <= 0)
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

        private float CalculateCharge(float deltaTime)
        {
            float timeToCharge = 60f;
            float creditsPerSecond = 100f / timeToCharge;
            float variance = chargeVariance * creditsPerSecond;

            float charge = rng.RangeFloat(creditsPerSecond - variance, creditsPerSecond + variance) * deltaTime;
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
    }

}