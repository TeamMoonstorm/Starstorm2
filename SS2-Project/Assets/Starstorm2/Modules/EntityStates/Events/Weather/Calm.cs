using SS2.Components;
using RoR2;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using SS2;
using RoR2.UI;
using System.Collections.Generic;
namespace EntityStates.Events
{
    public class Calm : GenericWeatherState
    {
        private static float chargeInterval = 30f;
        private static float chargeVariance = 0.5f; // higher difficulty = higher variance? (faster on average)
        private static float chargeFromKill = 0.33f;

        private float charge;
        private Xoroshiro128Plus rng;
        private Xoroshiro128Plus mobRng;
        private float chargeStopwatch = 10f;

        private float chargeFromKills;
        private float chargeFromTime;

        private float totalMultiplier;
        public override void OnEnter()
        {
            base.OnEnter();

            rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
            mobRng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
            this.totalMultiplier = rng.RangeFloat(1, 1 + chargeVariance);
            GlobalEventManager.onCharacterDeathGlobal += AddCharge;

            this.stormController.SetStormLevel(0, 0);
        }



        private void AddCharge(DamageReport damageReport)
        {
            if (TeamMask.GetEnemyTeams(TeamIndex.Player).HasTeam(damageReport.victimTeamIndex))
            {
                float charge = chargeFromKill;
                float variance = chargeVariance * charge;

                float charge1 = mobRng.RangeFloat(charge - variance, charge + variance) * totalMultiplier;
                this.chargeFromKills += charge1;
                this.charge += charge1;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active) return;

            this.chargeStopwatch -= Time.fixedDeltaTime;
            if (this.chargeStopwatch <= 0)
            {
                this.chargeStopwatch += chargeInterval;
                this.charge += CalculateCharge(chargeInterval);
            }

            this.stormController.SetStormLevel(0, this.charge);

            if (charge >= 100f)
            {
                outer.SetNextState(new Storm(stormLevel: 1, lerpDuration: 15f));
                return;
            }

        }

        private float CalculateCharge(float deltaTime)
        {
            //https://www.desmos.com/calculator/ara4m3dsz8
            //float y = 0.9f * Mathf.Log(3f * Stage.instance.entryDifficultyCoefficient + 2.4f) - 1.5f;
            //float sevenMinusY = 7 - y;
            //float creditsPerSecond = 100f / sevenMinusY;

            //https://www.desmos.com/calculator/po2mvubvzk
            float stageCount = Run.instance.stageClearCount + 1;
            float minutesToStorm = (-6f * stageCount) / (stageCount + 1) + 10f;
            float creditsPerSecond = 100f / (minutesToStorm * 60f);
            float variance = chargeVariance * creditsPerSecond;

            float charge = rng.RangeFloat(creditsPerSecond - variance, creditsPerSecond + variance) * deltaTime * totalMultiplier;
            this.chargeFromTime += charge;
            return charge;

        }

        public override void OnExit()
        {
            base.OnExit();
            GlobalEventManager.onCharacterDeathGlobal -= AddCharge;

            SS2Log.Info($"Storm level finished in {fixedAge} seconds.");
            SS2Log.Info($"Charge from kills = {chargeFromKills}");
            SS2Log.Info($"Charge form time = {chargeFromTime}");
        }
    }
}