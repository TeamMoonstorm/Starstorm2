using Moonstorm;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class ScepterBarrageFire : BaseState
    {
        public float charge;

        [TokenModifier("SS2_NEMMANDO_SPECIAL_SCEPSUBMISSION_DESCRIPTION", StatTypes.MultiplyByN, 2, "100")]
        public static float laserDamageCoefficient;
        public static float laserBlastRadius;// = 8f;
        public static float laserBlastForce;// = 2000f;

        [TokenModifier("SS2_NEMMANDO_SPECIAL_SCEPSUBMISSION_DESCRIPTION", StatTypes.MultiplyByN, 1, "100")]
        public static float damageCoefficient = 0.6f;
        public static float procCoefficient = 0.5f;
        public static uint bulletCountPerShot = 4;
        public static float range = 128f;
        public static float maxSpread = 40f;
        public static int minBulletCount = 2;
        [TokenModifier("SS2_NEMMANDO_SPECIAL_SCEPSUBMISSION_DESCRIPTION", StatTypes.Default, 0)]
        public static int maxBulletCount = 6;

        public static float baseDuration = 0.8f;
        public static float minTimeBetweenShots = 0.2f;
        public static float maxTimeBetweenShots = 0.075f;
        public static float recoil = 5f;
        public static GameObject tracerPrefab;

        private int totalBulletsFired;
        private int bulletCount;
        public float stopwatchBetweenShots;
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
            durationBetweenShots = (Util.Remap(charge, 0f, 1f, minTimeBetweenShots, maxTimeBetweenShots)) / attackSpeedStat;
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
            PlayCrossfade("UpperBody, Override", "Special", "Special.rate", durationBetweenShots, 0.05f);
            Util.PlaySound("NemmandoSubmissionFire", gameObject);
            Util.PlaySound(EntityStates.GolemMonster.FireLaser.attackSoundString, gameObject);

            float recoil = ScepterBarrageFire.recoil / attackSpeedStat;
            AddRecoil(-0.8f * recoil, -1f * recoil, -0.1f * recoil, 0.15f * recoil);

            EffectManager.SimpleMuzzleFlash(EntityStates.GolemMonster.FireLaser.effectPrefab, gameObject, "Muzzle", false);

            if (isAuthority)
            {
                new BulletAttack
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
                    tracerEffectPrefab = tracerPrefab, //tracerEffectPrefab = Projectiles.laserTracer,
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
                }.Fire();

                FireLaser();
            }

            characterBody.AddSpreadBloom(2f * EntityStates.Commando.CommandoWeapon.FireBarrage.spreadBloomValue);
            totalBulletsFired++;
        }

        private void FireLaser()
        {
            Ray aimRay = GetAimRay();
            Vector3 blastPosition = aimRay.origin + aimRay.direction * 1000f;

            RaycastHit raycastHit;
            if (Physics.Raycast(aimRay, out raycastHit, 1000f, LayerIndex.world.mask | LayerIndex.defaultLayer.mask | LayerIndex.entityPrecise.mask))
            {
                blastPosition = raycastHit.point;
            }
            //SS2Log.Debug(gameObject + " | " + TeamComponent.GetObjectTeam(gameObject) + " | " + teamComponent);
            //SS2Log.Debug(laserDamageCoefficient);

            BlastAttack blast = new BlastAttack
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                baseDamage = damageStat * laserDamageCoefficient,
                baseForce = laserBlastForce * 0.2f,
                position = blastPosition,
                radius = laserBlastRadius,
                falloffModel = BlastAttack.FalloffModel.SweetSpot,
                bonusForce = laserBlastForce * aimRay.direction,
                crit = RollCrit()
            };
            //DamageAPI.AddModdedDamageType(blast, Gouge.gougeDamageType);

            blast.Fire();

            if (modelTransform)
            {
                ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    int childIndex = childLocator.FindChildIndex("Muzzle");
                    if (EntityStates.GolemMonster.FireLaser.tracerEffectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = blastPosition,
                            start = aimRay.origin
                        };

                        effectData.SetChildLocatorTransformReference(gameObject, childIndex);

                        EffectManager.SpawnEffect(EntityStates.GolemMonster.FireLaser.tracerEffectPrefab, effectData, true);
                        EffectManager.SpawnEffect(EntityStates.GolemMonster.FireLaser.hitEffectPrefab, effectData, true);
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            //if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
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