using SS2.Components;
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
        private float tickTimer;
        private SelfRepairController selfRepairController;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            if (!gameObject.TryGetComponent(out selfRepairController))
            {
                Debug.LogError("SelfRepair: Missing SelfRepairController on " + gameObject.name);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active && selfRepairController != null)
            {
                tickTimer -= Time.fixedDeltaTime;
                if (tickTimer <= 0f && selfRepairController.repair >= repairCostPerTick)
                {
                    tickTimer = tickInterval;
                    characterBody.healthComponent.HealFraction(healthFractionPerTick, default);
                    selfRepairController.AddRepair(-repairCostPerTick);
                }
            }

            if (isAuthority && (fixedAge >= duration || selfRepairController == null || selfRepairController.repair < repairCostPerTick))
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
