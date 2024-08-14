using SS2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MSU;

namespace EntityStates.Knight
{
    class ShieldPunch : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        public int swingSide;

        public float hopVelocity = 15f;
        public float airControl = 0.30f;
        public float upwardVelocity = 0.5f;
        public float forwardVelocity = 15f;
        public float minimumY = 0.05f;
        public float aimVelocity = 1f;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!characterBody.HasBuff(SS2Content.Buffs.bdKnightShieldCooldown))
            {
                characterBody.AddTimedBuff(SS2Content.Buffs.bdKnightShieldCooldown, 1f);
                animator = GetModelAnimator();

                Vector3 direction = GetAimRay().direction;

                // Launch Knight where they are aiming
                if (isAuthority)
                {
                    characterBody.isSprinting = true;
                    direction.y = Mathf.Max(direction.y, minimumY);
                    Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                    Vector3 b = Vector3.up * upwardVelocity;
                    Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                    characterMotor.Motor.ForceUnground();
                    characterMotor.velocity = a + b + b2;
                    SmallHop(characterMotor, hopVelocity);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(SS2.Survivors.Knight.dodgeFOV, 60f, base.fixedAge / duration);
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "ShieldPunch", "Primary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override void OnExit()
        {
            skillLocator.primary.UnsetSkillOverride(skillLocator.primary, Shield.shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}