using SS2.Components;
using SS2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Toolbot
{
    public class SelfRepair : BaseSkillState
    {
        public static float baseDuration = 3f;
        public static float healthFractionPerTick = 0.05f;
        public static float repairCostPerTick = 1f;
        public static float tickInterval = 0.5f;

        private float duration;
        private float scaledTickInterval;
        private float tickTimer;
        private int ticksRemaining = 6;
        private bool repairFinished;
        private SelfRepairController selfRepairController;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            scaledTickInterval = tickInterval / attackSpeedStat;

            if (!gameObject.TryGetComponent(out selfRepairController))
            {
                SS2Log.Error("SelfRepair: Missing SelfRepairController on " + gameObject.name);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!repairFinished)
            {
                repairFinished = !selfRepairController || !selfRepairController.CanRepair(repairCostPerTick);
            }

            if (NetworkServer.active && !repairFinished)
            {
                tickTimer -= Time.fixedDeltaTime;
                // Keep the six-pulse budget even when several pulses fall within one physics tick.
                while (tickTimer <= 0f && ticksRemaining > 0 && !repairFinished)
                {
                    tickTimer += scaledTickInterval;
                    ticksRemaining--;
                    selfRepairController.AddRepair(-repairCostPerTick);
                    characterBody.healthComponent.HealFraction(healthFractionPerTick, default);
                    repairFinished = !selfRepairController.CanRepair(repairCostPerTick);
                }
            }

            repairFinished |= fixedAge >= duration;
            if (isAuthority && repairFinished)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
