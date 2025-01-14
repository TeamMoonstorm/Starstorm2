using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2.Survivors;
using R2API;

namespace EntityStates.Engi
{
    public class FireLaserFocus : BaseSkillState
    {
        [SerializeField]
        public float hitsToProcDebuff = 4f;
        [SerializeField]
        public float maxDistance = 25f;
        [SerializeField]
        public float extendedVisualDistance = 10f; //to make the beam feel a bit nicer
        [SerializeField]
        public float force;
        [SerializeField]
        public uint bulletCount;
        [SerializeField]
        public string animationLayerName;
        [SerializeField]
        public string animationStateName;
        [SerializeField]
        public string animationBapStateName;
        [SerializeField]
        public string animationExitStateName;
        [SerializeField]
        public static float damageCoeff = .75f;
        [SerializeField]
        public static float procCoeff = 0.3f;
        [SerializeField]
        public static float tickRate = 6f;
        [SerializeField]
        public static float baseMinimumDuration = 2f;
        [SerializeField]
        public static GameObject laserPrefab;
        [SerializeField]
        public static GameObject hitsparkPrefab;

        private float minimumDuration;

        private float fireTimer;
        private float bapTimer;
        private Transform modelTransform;
        private float counter = 0;

        private GameObject leftLaserInstance;
        private Transform leftLaserInstanceEnd;
        private ChildLocator leftCL;
        private AnimateShaderAlpha asaLeft;
        private Transform muzzleLeft;

        private GameObject rightLaserInstance;
        private Transform rightLaserInstanceEnd;
        private ChildLocator rightCL;
        private Transform muzzleRight;
        private AnimateShaderAlpha asaRight;

        private Ray aimRay;

        public override void OnEnter()
        {
            base.OnEnter();
            minimumDuration = baseMinimumDuration / attackSpeedStat;
            aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);
            PlayAnimation(animationLayerName, animationStateName);
            Util.PlaySound("Play_engi_R_walkingTurret_laser_start", base.gameObject);

            fireTimer = 0;
            counter = 0;
            modelTransform = base.GetModelTransform();

            //Creates laser
            if (modelTransform)
            {
                ChildLocator component = this.modelTransform.GetComponent<ChildLocator>();
                if (component)
                {
                    muzzleLeft = component.FindChild("MuzzleLeft");
                    if (muzzleLeft && laserPrefab)
                    {
                        leftLaserInstance = UnityEngine.Object.Instantiate<GameObject>(laserPrefab, muzzleLeft.position, muzzleLeft.rotation);
                        leftLaserInstance.transform.parent = transform;
                        leftCL = leftLaserInstance.GetComponent<ChildLocator>();
                        leftLaserInstanceEnd = leftCL.FindChild("LaserEnd");
                        asaLeft = leftCL.FindChild("MainLaser").GetComponent<AnimateShaderAlpha>();
                    }
                    muzzleRight = component.FindChild("MuzzleRight");
                    if (muzzleRight && laserPrefab)
                    {
                        rightLaserInstance = UnityEngine.Object.Instantiate<GameObject>(laserPrefab, muzzleRight.position, muzzleRight.rotation);
                        rightLaserInstance.transform.parent = transform;
                        rightCL = rightLaserInstance.GetComponent<ChildLocator>();
                        rightLaserInstanceEnd = rightCL.FindChild("LaserEnd");
                        asaRight = rightCL.FindChild("MainLaser").GetComponent<AnimateShaderAlpha>();
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            Ray aimRay = base.GetAimRay();
            StartAimMode(aimRay, 2f, false);

            fireTimer -= Time.fixedDeltaTime;
            bapTimer -= Time.fixedDeltaTime;
            if (fireTimer <= 0)
            {
                fireTimer = 1f / tickRate / attackSpeedStat;
                ++counter;
                FireLasers(aimRay);
            }

            if (leftLaserInstance && leftLaserInstanceEnd)
            {
                leftLaserInstance.transform.position = muzzleLeft.position;
                leftLaserInstance.transform.rotation = muzzleLeft.rotation;
                leftLaserInstanceEnd.position = aimRay.GetPoint(maxDistance);
            }
            if (rightLaserInstance && rightLaserInstanceEnd)
            {
                rightLaserInstance.transform.position = muzzleRight.position;
                rightLaserInstance.transform.rotation = muzzleRight.rotation;
                rightLaserInstanceEnd.position = aimRay.GetPoint(maxDistance);
            }

            if (((fixedAge >= minimumDuration && !IsKeyDownAuthority()) || characterBody.isSprinting) && isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }

            counter = counter % hitsToProcDebuff;
        }

        //Never move origin of bullet
        //Zenith made me make it so its two instances of damage
        private void FireLasers(Ray aimRay)
        {
            var isCrit = base.RollCrit();

            if (base.isAuthority)
            {
                var bullet = new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = base.characterBody.spreadBloomAngle,
                    bulletCount = this.bulletCount,
                    procCoefficient = procCoeff,
                    damage = (base.characterBody.damage * damageCoeff) / 2,
                    force = this.force,
                    falloffModel = BulletAttack.FalloffModel.None,
                    isCrit = isCrit,
                    HitEffectNormal = false,
                    smartCollision = true,
                    maxDistance = maxDistance,
                    radius = 0.6f
                };

                DamageAPI.AddModdedDamageType(bullet, SS2.Survivors.Engineer.EngiFocusDamage);
                bullet.Fire();
                //Trigger Proc
                if (counter % hitsToProcDebuff == 0)
                {
                    asaLeft.pauseTime = false;
                    asaLeft.time = 0;
                    asaRight.pauseTime = false;
                    asaRight.time = 0;
                    DamageAPI.AddModdedDamageType(bullet, SS2.Survivors.Engineer.EngiFocusDamageProc);
                    PlayAnimation(animationLayerName, animationBapStateName);
                    bullet.hitEffectPrefab = hitsparkPrefab;
                }

                bullet.Fire();

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound("Play_engi_R_walkingTurret_laser_end", gameObject);
            PlayAnimation(animationLayerName, animationExitStateName);

            if (leftLaserInstance)
                EntityState.Destroy(leftLaserInstance);

            if (rightLaserInstance)
                EntityState.Destroy(rightLaserInstance);

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
