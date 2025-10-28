using MSU;
using RoR2;
using UnityEngine;

namespace EntityStates.Executioner
{
    public class FirePistol : BaseSkillState
    {
        [FormatToken("SS2_EXECUTIONER_PISTOL_DESCRIPTION", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        private static float damageCoefficient = 1.8f;
        private static float procCoefficient = 1f;
        private static float baseDuration = 0.42f;
        private static float recoil = 2f;
        private static float spreadBloom = 0.75f;
        private static float force = 120f;

        private static float bulletRadius = .7f;
        [HideInInspector]
        private static GameObject muzzleEffectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/Muzzleflash1");
        [HideInInspector]
        private static GameObject tracerPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/tracercommandodefault");
        [HideInInspector]
        private static GameObject hitPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/HitsparkCommando");

        private float duration;
        private float fireDuration;
        private string muzzleString;
        private bool hasFired;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.1f * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            hasFired = false;
            PlayAnimation("Gesture, Override", "Primary");
            Shoot();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge < duration || !isAuthority)
                return;
            outer.SetNextStateToMain();
        }

        private void Shoot()
        {
            if (!hasFired)
            {
                hasFired = true;
                bool isCrit = RollCrit();

                string soundString = "ExecutionerPrimary";
                if (isCrit) soundString += "Crit";
                Util.PlaySound(soundString, gameObject);
                AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

                if (muzzleEffectPrefab)
                    EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleString, false);

                if (isAuthority)
                {
                    float dmg = damageCoefficient * damageStat;
                    Ray r = GetAimRay();

                    DamageTypeCombo damageType = DamageType.Generic;
                    damageType.damageSource = DamageSource.Primary;
                    BulletAttack bullet = new BulletAttack
                    {
                        aimVector = r.direction,
                        origin = r.origin,
                        damage = damageCoefficient * damageStat,
                        damageType = damageType,
                        damageColorIndex = DamageColorIndex.Default,
                        minSpread = 0f,
                        maxSpread = characterBody.spreadBloomAngle,
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        force = force,
                        isCrit = isCrit,
                        owner = gameObject,
                        muzzleName = muzzleString,
                        smartCollision = true,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = bulletRadius,
                        weapon = gameObject,
                        tracerEffectPrefab = tracerPrefab,
                        hitEffectPrefab = hitPrefab
                    };
                    bullet.Fire();
                }
                characterBody.AddSpreadBloom(spreadBloom);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}