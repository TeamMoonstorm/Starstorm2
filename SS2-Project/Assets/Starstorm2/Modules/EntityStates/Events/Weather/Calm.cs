#region OLD

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

        private bool oldStorm;
        private float oldDuration;

        public float thing = 1f;
        public override void OnEnter()
        {
            if (!SS2.Events.EnableEvents.value)
            {
                Destroy(base.gameObject);
                return;
            }
            base.OnEnter();
            this.stormController.StartLerp(0, 8f);

            this.totalMultiplier = StormController.chargeRng.RangeFloat(1, 1 + chargeVariance);
            GlobalEventManager.onCharacterDeathGlobal += AddCharge;

            oldDuration = fuckingduration() * 60f * thing;
            oldStorm = !SS2.Storm.ReworkedStorm.value;


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

            if (oldStorm)
            {
                if (base.fixedAge >= oldDuration && ShouldCharge())
                {
                    outer.SetNextState(new Storm { stormLevel = 3, lerpDuration = 10f });
                }
            }
            else
            {
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
                    this.stormController.OnStormLevelCompleted();
                    outer.SetNextState(new Storm { stormLevel = 1, lerpDuration = 15f });
                    return;
                }
            }


        }

        private float fuckingduration()
        {
            if (Run.instance.stageClearCount < 2) return Mathf.Infinity; // >:(
            if (UnityEngine.Random.Range(0f, 1f) > 0.6f) return Mathf.Infinity; // idk
            float stageCount = Run.instance.stageClearCount + 1;
            float minutesToStorm = (-13f * stageCount) / (stageCount + 1) + 13f;
            float variance = chargeVariance * minutesToStorm;
            return Mathf.Max(StormController.chargeRng.RangeFloat(minutesToStorm - variance, minutesToStorm + variance) * totalMultiplier, 1f);
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
//namespace EntityStates.Events
//{
//    public class Calm : GenericWeatherState
//    {
//        public float duration;
//        public override void OnEnter()
//        {
//            base.OnEnter();
//            this.stormController.StartLerp(0, 8f);
//        }

//        public override void FixedUpdate()
//        {
//            base.FixedUpdate();
//            if (!NetworkServer.active) return;

//            if (!stormController.hasStarted && ShouldCharge() && stormController.stormStartTime.hasPassed)
//            {
//                this.stormController.OnStormLevelCompleted();
//                outer.SetNextState(new Storm { stormLevel = 1, lerpDuration = 15f });
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
//        }
//    }
//}
#endregion
