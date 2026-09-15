using RoR2;
using UnityEngine;

namespace EntityStates.Duke
{
    public class FireADSShot : BaseSkillState
    {
        private static float damageCoefficient = 4f;
        private static float procCoefficient = 1f;
        private static float baseDuration = 0.4f;
        private static float recoil = 4f;
        private static float spreadBloom = 0.3f;
        private static float force = 300f;
        private static float bulletRadius = 0.4f;
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
            PlayAnimation("Gesture, Override", "FireADS");
            Fire();
        }

        private void Fire()
        {
            if (hasFired) return;
            hasFired = true;

            if (skillLocator.secondary.stock <= 0) return;
            skillLocator.secondary.DeductStock(1);
            characterBody.OnSkillActivated(skillLocator.secondary);

            bool isCrit = RollCrit();
            Util.PlayAttackSpeedSound("Play_railgunner_m1_fire", gameObject, attackSpeedStat);
            AddRecoil(-0.6f * recoil, -1f * recoil, -0.3f * recoil, 0.3f * recoil);

            if (muzzleEffectPrefab)
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleString, false);

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();

                DamageTypeCombo damageType = DamageType.Generic;
                damageType.damageSource = DamageSource.Secondary;

                var bulletAttack = new BulletAttack
                {
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = damageCoefficient * damageStat,
                    damageType = damageType,
                    damageColorIndex = DamageColorIndex.Default,
                    minSpread = 0f,
                    maxSpread = 0f,
                    falloffModel = BulletAttack.FalloffModel.None,
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
                    hitEffectPrefab = hitPrefab,
                    sniper = true // BulletAttack auto-crits on sniper target hits (sets isCrit + DamageColorIndex.Sniper)
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
