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
        private float chargeStopwatch = 10f;

        private float chargeFromKills;
        private float chargeFromTime;

        private float totalMultiplier;
        public override void OnEnter()
        {
            base.OnEnter();
            this.stormController.SetEffectIntensity(0);
            if (stormController.AttemptSkip()) // TEMPORARY. FUUUUUUUUUCK
            {
                EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("EtherealStormWarning", SS2Bundle.Events), new EffectData { }, true);
                this.outer.SetNextState(new Storm { stormLevel = 1, lerpDuration = 15f });
                return;
            }


            this.totalMultiplier = StormController.chargeRng.RangeFloat(1, 1 + chargeVariance);
            GlobalEventManager.onCharacterDeathGlobal += AddCharge;

            
        }



        private void AddCharge(DamageReport damageReport)
        {
            if (ShouldCharge() && TeamMask.GetEnemyTeams(TeamIndex.Player).HasTeam(damageReport.victimTeamIndex))
            {
                float charge = chargeFromKill;
                float variance = chargeVariance * charge;

                float charge1 = StormController.mobChargeRng.RangeFloat(charge - variance, charge + variance) * totalMultiplier;
                this.chargeFromKills += charge1;
                this.charge += charge1;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active) return;

            this.chargeStopwatch -= Time.fixedDeltaTime;
            if (this.chargeStopwatch <= 0 && ShouldCharge())
            {
                this.chargeStopwatch += chargeInterval;
                float f = CalculateCharge(chargeInterval);
                this.charge += f;
                this.stormController.AddCharge(f);
            }

            if (charge >= 100f)
            {
                outer.SetNextState(new Storm { stormLevel = 1, lerpDuration = 15f });
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

            float charge = StormController.chargeRng.RangeFloat(creditsPerSecond - variance, creditsPerSecond + variance) * deltaTime * totalMultiplier;
            this.chargeFromTime += charge;
            return charge;

        }

        private bool ShouldCharge()
        {
            bool shouldCharge = !TeleporterInteraction.instance;
            shouldCharge |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle;
            return shouldCharge;
        }

        public override void OnExit()
        {
            base.OnExit();
            GlobalEventManager.onCharacterDeathGlobal -= AddCharge;

            SS2Log.Info($"Storm level finished in {fixedAge} seconds.");
            SS2Log.Info($"Charge from kills = {chargeFromKills}");
            SS2Log.Info($"Charge form time = {chargeFromTime}");
            SS2Log.Info($"TotalMultiplier = {totalMultiplier}");
        }
    }
}