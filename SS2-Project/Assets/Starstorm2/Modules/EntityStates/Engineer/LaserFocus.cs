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
    public class LaserFocus : BaseSkillState
    {
        [SerializeField]
        public static float damageCoeff = .75f;

        [SerializeField]
        public static float procCoeff = 0.5f;

        [SerializeField]
        public static float fireFrequency = 4f;

        [SerializeField]
        public static float maxDistance = 25f;

        public float baseDuration = 1f;
        private float duration;
        private float fireTimer;
        private Transform modelTransform;
        private List<HealthComponent> boostedTargets;
        //MuzzleLeft
        //MuzzleRight

        //public GameObject hitEffectPrefab = FireBarrage.hitEffectPrefab;
        //public GameObject tracerEffectPrefab = FireBarrage.tracerEffectPrefab;

        public GameObject hitsparkPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Hitspark1.prefab").WaitForCompletion();
        public GameObject laserPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/LaserEngiTurret.prefab").WaitForCompletion();

        private GameObject leftLaserInstance;
        private Transform leftLaserInstanceEnd;

        private GameObject rightLaserInstance;
        private Transform rightLaserInstanceEnd;

        private Transform muzzleLeft;
        private Transform muzzleRight;


        public override void OnEnter()
        {
            base.OnEnter();
            //this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            this.PlayAnimation("Gesture, Additive", "ChargeGrenades");

            Util.PlaySound("Play_engi_R_walkingTurret_laser_start", base.gameObject);
            fireTimer = 0;
            boostedTargets = new List<HealthComponent>();
            modelTransform = base.GetModelTransform();
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
                        leftLaserInstanceEnd = leftLaserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
                    }
                    muzzleRight = component.FindChild("MuzzleRight");
                    if (muzzleRight && laserPrefab)
                    {
                        rightLaserInstance = UnityEngine.Object.Instantiate<GameObject>(laserPrefab, muzzleRight.position, muzzleRight.rotation);
                        rightLaserInstance.transform.parent = transform;
                        rightLaserInstanceEnd = rightLaserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
                    }
                }
            }
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //Stop_engi_R_walkingTurret_laser_loop
            //Play_engi_R_walkingTurret_laser_end

            Util.PlaySound("Stop_engi_R_walkingTurret_laser_loop", base.gameObject);

            //this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            fireTimer += Time.fixedDeltaTime;
            float fireRate = fireFrequency * base.characterBody.attackSpeed;
            float rateAdjusted = 1f / fireRate;
            if (fireTimer > rateAdjusted)
            {
                FireLasers(aimRay);
                fireTimer = 0f;
            }

            if (leftLaserInstance && leftLaserInstanceEnd)
            {
                leftLaserInstance.transform.position = muzzleLeft.position;
                leftLaserInstance.transform.rotation = muzzleLeft.rotation;
                leftLaserInstanceEnd.position = GetBeamEndPoint(aimRay);
            }
            if(rightLaserInstance && rightLaserInstanceEnd)
            {
                rightLaserInstance.transform.position = muzzleRight.position;
                rightLaserInstance.transform.rotation = muzzleRight.rotation;
                rightLaserInstanceEnd.position = GetBeamEndPoint(aimRay);
            }

            if (this.isAuthority &&!inputBank.skill1.down)
            {
                this.outer.SetNextStateToMain();
                return;
            }

            //if (base.fixedAge >= this.duration && base.isAuthority)
            //{
            //    this.outer.SetNextStateToMain();
            //    return;
            //}
        }

        protected Vector3 GetBeamEndPoint(Ray aimRay)
        {
            Vector3 point = aimRay.GetPoint(maxDistance);
            RaycastHit raycastHit;
            
            if (Util.CharacterRaycast(base.gameObject, aimRay, out raycastHit, maxDistance, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
            {
                point = raycastHit.point;
            }
            return point;
        }

        private void FireLasers(Ray aimRay)
        {
            var isCrit = base.RollCrit();

            if (base.isAuthority)
            {
                if (isCrit)
                {
                    // Do the thing swuff mentioned
                }

                var bulletLeft = new BulletAttack {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = base.characterBody.spreadBloomAngle,
                    bulletCount = 1U,
                    procCoefficient = procCoeff,
                    damage = (base.characterBody.damage * damageCoeff) / 2,
                    force = 0,
                    falloffModel = BulletAttack.FalloffModel.None,
                    muzzleName = "MuzzleLeft",
                    hitEffectPrefab = hitsparkPrefab,
                    isCrit = isCrit,
                    HitEffectNormal = false,
                    smartCollision = true,
                    maxDistance = maxDistance,
                    radius = 0.7f
                };

                var bulletRight = new BulletAttack{
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = base.characterBody.spreadBloomAngle,
                    bulletCount = 1U,
                    procCoefficient = procCoeff,
                    damage = (base.characterBody.damage * damageCoeff) / 2,
                    force = 0,
                    falloffModel = BulletAttack.FalloffModel.None,
                    muzzleName = "MuzzleRight",
                    hitEffectPrefab = hitsparkPrefab,
                    isCrit = isCrit,
                    HitEffectNormal = false,
                    smartCollision = true,
                    maxDistance = maxDistance,
                    radius = 0.7f
                };

                DamageAPI.AddModdedDamageType(bulletLeft, Engineer.EngiFocusDamage);
                DamageAPI.AddModdedDamageType(bulletRight, Engineer.EngiFocusDamage);


                bulletLeft.Fire();
                bulletRight.Fire();

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound("Play_engi_R_walkingTurret_laser_end", base.gameObject);
            this.PlayAnimation("Gesture, Additive", "Empty");

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
