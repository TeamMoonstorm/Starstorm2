using Moonstorm;
using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class BarrageFire : RendHandler
    {
        [TokenModifier("SS2_NEMMANDO_SPECIAL_SUBMISSION_DESCRIPTION", StatTypes.MultiplyByN, 1, "100")]
        public static float damageCoefficient;
        public static float procCoefficient;
        public static uint bulletCountPerShot;
        public static float range;
        public static float maxSpread;
        public static int minBulletCount;
        [TokenModifier("SS2_NEMMANDO_SPECIAL_SUBMISSION_DESCRIPTION", StatTypes.Default, 0)]
        public static int maxBulletCount;
        public static float baseDuration;
        public static float minTimeBetweenShots;
        public static float maxTimeBetweenShots;
        public static float recoil;
        public static GameObject tracerPrefab;

        private int totalBulletsFired;
        private int bulletCount;
        public float stopwatchBetweenShots;
        public float charge;
        private Animator modelAnimator;
        private Transform modelTransform;
        private float duration;
        private float durationBetweenShots;
        private GameObject muzzleFlashEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/FusionCellExplosion");

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetSpreadBloom(0.2f, false);
            characterBody.isSprinting = false;
            duration = baseDuration;
            durationBetweenShots = Util.Remap(charge, 0f, 1f, minTimeBetweenShots, maxTimeBetweenShots) / attackSpeedStat;
            bulletCount = (int)(Mathf.RoundToInt(Util.Remap(charge, 0f, 1f, minBulletCount, maxBulletCount)) * attackSpeedStat);
            modelAnimator = GetModelAnimator();
            modelTransform = GetModelTransform();
            characterBody.SetAimTimer(2f);
            characterBody.outOfCombatStopwatch = 0f;
            FireBullet();
        }

        private void FireBullet()
        {
            Ray aimRay = GetAimRay();
            string muzzleName = "Muzzle";

            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FireBarrage.effectPrefab, gameObject, muzzleName, false);
            Util.PlaySound("NemmandoSubmissionFire", gameObject);
            PlayCrossfade("Gesture, Additive", "FireGun", "FireGun.playbackRate", durationBetweenShots, durationBetweenShots / 2f);
            AddRecoil(-0.8f * recoil, -1f * recoil, -0.1f * recoil, 0.15f * recoil);

            if (isAuthority)
            {
                // this effect was too noisy, will have to trim it
                /*EffectManager.SpawnEffect(muzzleFlashEffect, new EffectData
                {
                    origin = base.FindModelChild(muzzleName).position,
                    scale = 1f
                }, true);*/

                BulletAttack bulletAttack = new BulletAttack
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0,
                    maxSpread = maxSpread,
                    bulletCount = bulletCountPerShot,
                    damage = damageCoefficient * damageStat,
                    force = 0.5f * EntityStates.Commando.CommandoWeapon.FireBarrage.force,
                    tracerEffectPrefab = tracerPrefab,
                    muzzleName = muzzleName,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FireBarrage.hitEffectPrefab,
                    isCrit = RollCrit(),
                    radius = EntityStates.Commando.CommandoWeapon.FireBarrage.bulletRadius,
                    smartCollision = true,
                    damageType = DamageType.Generic,
                    spreadPitchScale = 0.5f,
                    spreadYawScale = 0.5f,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = range
                };

                RendMultiplier(bulletAttack);

                bulletAttack.Fire();
            }

            characterBody.AddSpreadBloom(2f * EntityStates.Commando.CommandoWeapon.FireBarrage.spreadBloomValue);
            totalBulletsFired++;
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayCrossfade("Gesture, Override", "BufferEmpty", "", 1f, .4f); ;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatchBetweenShots += Time.fixedDeltaTime;

            if (stopwatchBetweenShots >= durationBetweenShots && totalBulletsFired < bulletCount)
            {
                stopwatchBetweenShots -= durationBetweenShots;
                FireBullet();
            }

            if (fixedAge >= duration && totalBulletsFired == bulletCount && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}