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
        public GameObject muzzleflashEffectPrefab;
        [SerializeField]
        public float maxDistance = 25f;
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
        public float trajectoryAimAssistMultiplier = 0.75f;
        [SerializeField]
        public static float damageCoeff = .75f;
        [SerializeField]
        public static float procCoeff = 0.3f;
        [SerializeField]
        public static float fireFrequency = 4f;

        private float fireTimer;
        private Transform modelTransform;
        private float counter = 0;

        public GameObject hitsparkPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Hitspark1.prefab").WaitForCompletion();
        public GameObject hitsparkPrefab2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/HitsparkRailgunnerPistol.prefab").WaitForCompletion();
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
            Ray aimRay = base.GetAimRay();
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

            Util.PlaySound("Stop_engi_R_walkingTurret_laser_loop", base.gameObject);

            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            fireTimer += Time.fixedDeltaTime;
            float frequencyAdjusted = fireFrequency * base.characterBody.attackSpeed;
            float interval = 1f / frequencyAdjusted;
            if (fireTimer > interval)
            {
                ++counter;
                FireLasers(aimRay);
                fireTimer = 0f;
            }

            if (leftLaserInstance && leftLaserInstanceEnd)
            {
                leftLaserInstance.transform.position = muzzleLeft.position;
                leftLaserInstance.transform.rotation = muzzleLeft.rotation;
                leftLaserInstanceEnd.position = GetBeamEndPoint(aimRay);
            }
            if (rightLaserInstance && rightLaserInstanceEnd)
            {
                rightLaserInstance.transform.position = muzzleRight.position;
                rightLaserInstance.transform.rotation = muzzleRight.rotation;
                rightLaserInstanceEnd.position = GetBeamEndPoint(aimRay);
            }

            if (this.isAuthority && !inputBank.skill1.down)
            {
                this.outer.SetNextStateToMain();
                return;
            }

            if (counter >= 8)
            {
                counter = 0;
            }
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

        //Never move origin of bullet
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
                    hitEffectPrefab = counter == 4 || counter == 8 ? hitsparkPrefab2 : hitsparkPrefab, // i would do % 4 but then the first hit procs it :(
                    isCrit = isCrit,
                    HitEffectNormal = false,
                    smartCollision = true,
                    maxDistance = maxDistance,
                    radius = 0.6f
                };

                DamageAPI.AddModdedDamageType(bullet, SS2.Survivors.Engineer.EngiFocusDamage);
                bullet.Fire();
                
                if (counter % 4 == 0)
                {
                    DamageAPI.AddModdedDamageType(bullet, SS2.Survivors.Engineer.EngiFocusDamageProc);
                    EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, gameObject, "MuzzleLeft", true);
                    EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, gameObject, "MuzzleRight", true);
                    PlayAnimation(animationLayerName, animationBapStateName);
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
