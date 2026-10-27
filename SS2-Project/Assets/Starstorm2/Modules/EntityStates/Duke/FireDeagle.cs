using RoR2;
using UnityEngine;

namespace EntityStates.Duke
{
    public class FireDeagle : BaseSkillState
    {
        private static float damageCoefficient = 1.8f;
        private static float procCoefficient = 1f;
        private static float baseDuration = 0.5f;
        private static float recoil = 3f;
        private static float spreadBloom = 0.5f;
        private static float force = 200f;
        private static float bulletRadius = 1f;
        private static float range = 256f;
        private static string muzzleString = "Muzzle";

        public static GameObject muzzleEffectPrefab;
        public static GameObject tracerPrefab;
        public static GameObject hitPrefab;

        private float duration;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(2f);
            hasFired = false;
            PlayAnimation("Gesture, Override", "Primary");
            Fire();
        }

        private void Fire()
        {
            if (hasFired) return;
            hasFired = true;

            bool isCrit = RollCrit();
            Util.PlayAttackSpeedSound("Play_bandit2_m1_rifle", gameObject, attackSpeedStat);
            AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

            if (muzzleEffectPrefab)
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleString, false);

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();

                DamageTypeCombo damageType = DamageType.Generic;
                damageType.damageSource = DamageSource.Primary;

                var bulletAttack = new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = damageCoefficient * damageStat,
                    damageType = damageType,
                    damageColorIndex = DamageColorIndex.Default,
                    minSpread = 0f,
                    maxSpread = characterBody.spreadBloomAngle,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = range,
                    force = force,
                    isCrit = isCrit,
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default,
                    procCoefficient = procCoefficient,
                    radius = bulletRadius,
                    weapon = gameObject,
                    tracerEffectPrefab = tracerPrefab,
                    hitEffectPrefab = hitPrefab
                };
                bulletAttack.Fire();
            }
            characterBody.AddSpreadBloom(spreadBloom);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
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
