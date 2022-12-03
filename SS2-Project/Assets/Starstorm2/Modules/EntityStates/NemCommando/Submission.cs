using Moonstorm;
using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class Submission : BaseSkillState
    {
        [TokenModifier("SS2_NEMMANDO_SUBMISSION_DESCRIPTION", StatTypes.MultiplyByN, 1, "100")]
        public static float damageCoefficient;
        public static float procCoefficient;
        public static uint bulletCountPerShot;
        public static float range;
        public static float maxSpread;
        [TokenModifier("SS2_NEMMANDO_SUBMISSION_DESCRIPTION", StatTypes.Default, 0)]
        public static int BulletCount;
        public static float baseDuration;
        //public static float timeBetweenShots;
        public static float recoil;
        public static GameObject tracerPrefab;
        public static float baseDurationBetweenShots;

        private int totalBulletsFired;
        private int bulletCount;
        private float durationBetweenShots;
        public float stopwatchBetweenShots;
        public float charge;
        private Animator modelAnimator;
        private Transform modelTransform;
        private float duration;
        
        private GameObject muzzleFlashEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/FusionCellExplosion");
        // ^ jesus wtf?

        [HideInInspector]
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetSpreadBloom(0.2f, false);
            duration = baseDuration;
            durationBetweenShots = baseDurationBetweenShots / attackSpeedStat;
            bulletCount = (int)(BulletCount * attackSpeedStat);
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

            EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FireBarrage.effectPrefab, gameObject, muzzleName, false);
            Util.PlaySound("NemmandoSubmissionFire", gameObject);
            if (modelAnimator.GetFloat("primaryPlaying") > 0.05) PlayCrossfade("Gesture, Additive, LeftArm", "FireGun", "FireGun.playbackRate", durationBetweenShots, durationBetweenShots / 2f);
            else PlayCrossfade("Gesture, Override, LeftArm", "FireGun", "FireGun.playbackRate", durationBetweenShots, durationBetweenShots / 2f);
            AddRecoil(-0.8f * recoil, -1f * recoil, -0.1f * recoil, 0.15f * recoil);

            if (isAuthority)
            {

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
                    force = 0.5f * Commando.CommandoWeapon.FireBarrage.force,
                    tracerEffectPrefab = tracerEffectPrefab,
                    muzzleName = muzzleName,
                    hitEffectPrefab = Commando.CommandoWeapon.FireBarrage.hitEffectPrefab,
                    isCrit = RollCrit(),
                    radius = Commando.CommandoWeapon.FireBarrage.bulletRadius,
                    smartCollision = true,
                    damageType = DamageType.Stun1s,
                    spreadPitchScale = 0.5f,
                    spreadYawScale = 0.5f,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = range
                };

                bulletAttack.Fire();
            }

            characterBody.AddSpreadBloom(2f * EntityStates.Commando.CommandoWeapon.FireBarrage.spreadBloomValue);
            totalBulletsFired++;
        }

        public override void OnExit()
        {
            base.OnExit();
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