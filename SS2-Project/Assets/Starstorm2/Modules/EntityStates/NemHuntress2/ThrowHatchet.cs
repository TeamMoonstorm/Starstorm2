using Moonstorm;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.NemHuntress2
{
    public class ThrowHatchet : BasicMeleeAttack
    {
        public float charge;

        public static float projectileDmgCoefficient;
        public static float projectileProcCoefficient;
        public static float projectileRecoilAmount;
        public static float projectileBaseSpeed;
        public static GameObject projectilePrefab;
        public static float hopVelocity;

        private float fireDuration;
        private bool hasFired;
        //private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            //animator = GetModelAnimator();
            hasFired = false;
            //duration = baseDuration / attackSpeedStat;
            fireDuration = 0.85f * duration;
        }

        public override void BeginMeleeAttackEffect()
        {
            base.BeginMeleeAttackEffect();
            FireProjectile();
            HopIfAirborne();
        }

        private void HopIfAirborne()
        {
            if (!characterMotor.isGrounded)
            {
                SmallHop(characterMotor, hopVelocity);
            }
        }

        public virtual void FireProjectile()
        {
            if (!hasFired)
            {
                hasFired = true;

                if (isAuthority)
                {
                    float damage = projectileDmgCoefficient * damageStat;
                    AddRecoil(-2f * projectileRecoilAmount, -3f * projectileRecoilAmount, -1f * projectileRecoilAmount, 1f * projectileRecoilAmount);
                    characterBody.AddSpreadBloom(0.33f * projectileRecoilAmount);
                    Ray aimRay = GetAimRay();

                    ProjectileManager.instance.FireProjectile(
                        projectilePrefab,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(aimRay.direction),
                        gameObject,
                        damage,
                        0f,
                        RollCrit(),
                        DamageColorIndex.Default,
                        null,
                        projectileBaseSpeed);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDuration)
            {
                //FireProjectile();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            //overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}