using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Engi
{
    public class LaserFocus : BaseSkillState
    {
        [SerializeField]
        public static float damageCoeff = 1f;

        [SerializeField]
        public static float procCoeff = 0.7f;

        [SerializeField]
        public float fireFrequency = 5f;

        [SerializeField]
        public float maxDistance = 50f;

        public float baseDuration = 1f;
        private float duration;
        private float fireTimer;
        private Transform modelTransform;
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

        public override void OnEnter()
        {
            Debug.Log("ENTER!!!!!!!!!!!!");
            base.OnEnter();
            this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            Debug.Log("HI IM ON ENTER");
            this.PlayAnimation("Gesture, Additive", "ChargeGrenades");

            Util.PlaySound("Play_engi_R_walkingTurret_laser_start", base.gameObject);
            fireTimer = 0;
            modelTransform = base.GetModelTransform();
            if (modelTransform){

                ChildLocator component = this.modelTransform.GetComponent<ChildLocator>();
                if (component){
                    Transform transformL = component.FindChild("LeftMuzzle");
                    if (transformL && laserPrefab){
                        leftLaserInstance = UnityEngine.Object.Instantiate<GameObject>(laserPrefab, transformL.position, transformL.rotation);
                        leftLaserInstance.transform.parent = transform;
                        leftLaserInstanceEnd = leftLaserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
                    }

                    Transform transformR = component.FindChild("RightMuzzle");
                    if (transformR && laserPrefab){
                        rightLaserInstance = UnityEngine.Object.Instantiate<GameObject>(laserPrefab, transformR.position, transformR.rotation);
                        rightLaserInstance.transform.parent = transform;
                        rightLaserInstanceEnd = rightLaserInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
                    }
                }
            }
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Debug.Log("heyt im fixed update ");
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

            if (leftLaserInstance && rightLaserInstance && leftLaserInstanceEnd && rightLaserInstanceEnd)
            {
                leftLaserInstanceEnd.position = GetBeamEndPoint(aimRay);
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
            
            if (Util.CharacterRaycast(base.gameObject, aimRay, out raycastHit, this.maxDistance, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
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

                var bulletLeft = new BulletAttack{
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
                    maxDistance = maxDistance
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
                    maxDistance = maxDistance
                };

                bulletLeft.Fire();
                bulletRight.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("im on exit.....");
            Util.PlaySound("Play_engi_R_walkingTurret_laser_end", base.gameObject);
            this.PlayAnimation("Gesture, Additive", "Empty");

            if (leftLaserInstance){
                EntityState.Destroy(leftLaserInstance);
            }
            if (rightLaserInstance)
            {
                EntityState.Destroy(rightLaserInstance);
            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
