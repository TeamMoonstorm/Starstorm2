using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Hero
{
    public class SpellCircles : BaseSkillState
    {
        public static GameObject spellObject;
        public static float baseDuration;
        public static float baseDurationBeforeCast;
        public static float baseDurationBetweenCast;
        private HuntressTracker tracker;
        private float duration;
        private float durationBeforeCast;
        private float durationBetweenCast;
        private float timer;
        private float castTimer;
        
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            durationBeforeCast = baseDurationBeforeCast / attackSpeedStat;
            durationBetweenCast = baseDurationBetweenCast / attackSpeedStat;
            tracker = GetComponent<HuntressTracker>();
            if (tracker == null)
            {
                outer.SetNextStateToMain();
            }
        }

        private void Circle()
        {
            if (tracker.trackingTarget)
            {
                if (tracker.trackingTarget.healthComponent.alive)
                {
                    ProjectileManager.instance.FireProjectile(spellObject, tracker.trackingTarget.transform.position, new Quaternion(0, 0, 0, 0), characterBody.gameObject, 1f, 0f, characterBody.RollCrit(), DamageColorIndex.Default, null, 0);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!NetworkServer.active)
                return;

            timer += Time.fixedDeltaTime;
            if (timer >= durationBeforeCast)
            {
                if (castTimer >= durationBetweenCast)
                {
                    Circle();
                    castTimer -= durationBetweenCast;
                }

                castTimer += Time.fixedDeltaTime;
            }

            if (timer >= duration)
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

