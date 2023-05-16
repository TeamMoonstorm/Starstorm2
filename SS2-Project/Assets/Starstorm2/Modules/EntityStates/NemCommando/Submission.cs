using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.NemCommando
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
        public static GameObject muzzleFlashPrefab;
        public static GameObject hitSparkPrefab;
        public static float baseDurationBetweenShots;

        private int totalBulletsFired;
        private int bulletCount;
        private float durationBetweenShots;
        public float stopwatchBetweenShots;
        public float charge;
        private Animator modelAnimator;
        private Transform modelTransform;
        private float duration;
        private string skinNameToken;
        
        private GameObject muzzleFlashEffect = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/FusionCellExplosion");
        // ^ jesus wtf?

        [HideInInspector]
        //public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

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

            skinNameToken = modelTransform.GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            if (skinNameToken != "SS2_SKIN_NEMCOMMANDO_DEFAULT" && skinNameToken != "SS2_SKIN_NEMCOMMANDO_GRANDMASTERY")
            {
                //Yellow
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_MASTERY")
                {
                    tracerPrefab = SS2Assets.LoadAsset<GameObject>("TracerNemCommandoShotgunYellow", SS2Bundle.NemCommando);
                    muzzleFlashPrefab = SS2Assets.LoadAsset<GameObject>("MuzzleflashNemCommandoYellow", SS2Bundle.NemCommando);
                    hitSparkPrefab = SS2Assets.LoadAsset<GameObject>("HitsparkNemCommandoYellow", SS2Bundle.NemCommando);
                }
                //Blue
                if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_COMMANDO")
                {
                    tracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoShotgun.prefab").WaitForCompletion();
                    muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion();
                    hitSparkPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommandoShotgun.prefab").WaitForCompletion();
                }
            }
            //Red
            else
            {
                tracerPrefab = SS2Assets.LoadAsset<GameObject>("TracerNemCommandoShotgunRed", SS2Bundle.NemCommando);
                muzzleFlashPrefab = SS2Assets.LoadAsset<GameObject>("MuzzleflashNemCommandoRed", SS2Bundle.NemCommando);
                hitSparkPrefab = SS2Assets.LoadAsset<GameObject>("HitsparkNemCommandoRed", SS2Bundle.NemCommando);
            }

            FireBullet();
        }

        private void FireBullet()
        {
            Ray aimRay = GetAimRay();
            string muzzleName = "Muzzle";

            EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, muzzleName, false);
            Util.PlaySound("NemmandoSubmissionFire", gameObject);

            if (modelAnimator.GetFloat("primaryPlaying") > 0.05) PlayCrossfade("Gesture, Override, LowerLeftArm", "FireGunSpecial", "Special.playbackRate", durationBetweenShots, durationBetweenShots / 2f);
            PlayCrossfade("Gesture, Override, LeftArm", "FireGunSpecial", "Special.playbackRate", durationBetweenShots, durationBetweenShots / 2f);
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
                    tracerEffectPrefab = tracerPrefab,
                    muzzleName = muzzleName,
                    hitEffectPrefab = hitSparkPrefab,
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

            //FindModelChild("casingParticle").GetComponent<ParticleSystem>().Emit((int)bulletCountPerShot);
            //lol

            characterBody.AddSpreadBloom(2f * Commando.CommandoWeapon.FireBarrage.spreadBloomValue);
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
            return InterruptPriority.PrioritySkill;
        }
    }
}