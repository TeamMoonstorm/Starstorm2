using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.NemHuntress2
{
    class GlaiveThrow : BaseSkillState
    {

        [SerializeField]
        public static float projectileDmgCoefficient;
        [SerializeField]
        public static float projectileProcCoefficient;
        [SerializeField]
        public static float projectileRecoilAmount;
        [SerializeField]
        public static float projectileBaseSpeed;
        [SerializeField]
        public static GameObject projectilePrefab;
        [SerializeField]
        public static float baseDuration;

        private bool hasFired;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            hasFired = false;
            if (characterMotor)
            {
                characterMotor.velocity.y = 3;
            }
            duration = baseDuration / attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            CharacterMotor characterMotor = base.characterMotor;
            characterMotor.velocity.y += 30 * Time.fixedDeltaTime * (1f - fixedAge / duration);

            if (fixedAge >= duration/2 && base.isAuthority)
            {
                FireProjectile();
            }

            if (fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
                return;
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


        public override void OnExit()
        {
            base.OnExit();
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

    }
}
