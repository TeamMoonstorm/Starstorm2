using Moonstorm;
using RoR2;
using UnityEngine;

namespace EntityStates.Executioner
{
    public class FirePistol : BaseSkillState
    {
        [TokenModifier("SS2_EXECUTIONER_PISTOL_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float recoil;
        public static float spreadBloom;
        public static float force;

        [HideInInspector]
        public static GameObject muzzleEffectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/Muzzleflash1");
        [HideInInspector]
        public static GameObject tracerPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/tracercommandodefault");
        [HideInInspector]
        public static GameObject hitPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/HitsparkCommando");

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
            PlayCrossfade("Gesture, Override", "Primary", "Primary.playbackRate", duration * 2.5f, 0.05f);
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


                    BulletAttack bullet = new BulletAttack
                    {
                        aimVector = r.direction,
                        origin = r.origin,
                        damage = damageCoefficient * damageStat,
                        damageType = DamageType.Generic,
                        damageColorIndex = DamageColorIndex.Default,
                        minSpread = 0f,
                        maxSpread = characterBody.spreadBloomAngle,
                        falloffModel = BulletAttack.FalloffModel.None,
                        force = force,
                        isCrit = isCrit,
                        owner = gameObject,
                        muzzleName = muzzleString,
                        smartCollision = true,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 0.35f,
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