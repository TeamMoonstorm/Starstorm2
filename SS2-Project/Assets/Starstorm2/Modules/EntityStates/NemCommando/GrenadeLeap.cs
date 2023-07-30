using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCommando
{
    public class GrenadeLeap : BaseSkillState
    {
        public static float baseDuration;
        private float duration;

        public static float airControl;
        private float previousAirControl;
        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float minimumY;
        public static float aimVelocity;

        private float throwDur;
        private bool hasThrown = false;

        public static GameObject projectilePrefab;
        public static float damageCoefficient;
        public static float projectileSpeed;

        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            throwDur = duration * 0.6f;

            characterBody.SetAimTimer(duration * 1.2f);

            animator = GetModelAnimator();

            PlayCrossfade("Body", "AltUtility", "Utility.rate", duration * 1.4f, 0.05f);

            Vector3 direction = -GetAimRay().direction;

            if (isAuthority)
            {
                characterBody.isSprinting = true;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 b = Vector3.up * upwardVelocity;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = a + b + b2;
            }
        }

        public override void FixedUpdate()
        {
            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }

            if (!hasThrown && animator.GetFloat("throwGrenades") >= 0.5f)
            {
                hasThrown = true;
                ThrowGrenades();
            }

            base.FixedUpdate();
        }

        public void ThrowGrenades()
        {
            Vector3 direction = GetAimRay().direction;
            Vector3 normalizedDirection = direction.normalized;
            Vector3 leftDirection = Quaternion.Euler(0f, -35f, 0f) * normalizedDirection;
            Vector3 rightDirection = Quaternion.Euler(0f, 35f, 0f) * normalizedDirection;

            Grenade(direction);
            Grenade(leftDirection);
            Grenade(rightDirection);
        }

        private void Grenade(Vector3 direction)
        {
            float damage = damageCoefficient * damageStat;
            Ray aimRay = GetAimRay();
            ProjectileManager.instance.FireProjectile(
                projectilePrefab,
                aimRay.origin,
                Util.QuaternionSafeLookRotation(direction),
                gameObject,
                damage,
                0f,
                RollCrit(),
                DamageColorIndex.Default,
                null,
                projectileSpeed);
        }

        public override void OnExit()
        {
            //ThrowGrenades();
            base.OnExit();
        }
    }
}
