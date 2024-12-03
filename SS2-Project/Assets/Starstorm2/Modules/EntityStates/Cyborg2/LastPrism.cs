using RoR2;
using UnityEngine;
using SS2.Components;
using System;
namespace EntityStates.Cyborg2
{
    public class LastPrism : BaseSkillState
    {
        private static float maxDamageCoefficientPerSecond = 10f;
        private static float procCoefficientPerSecond = 3f;
        private static float forcePerSecond = 500f;
        private static float baseTicksPerSecond = 6f;

        private static float baseMinimumDuration  = 0.25f;

        private static float maxRange = 50f;
        private static float bulletRadius = 1.5f;

        private static float minTurnAnglePerSecond = 90f;

        public static GameObject beamEffectPrefab;

        private static float walkSpeedPenaltyCoefficient = 0.33f;

        private float recoilAmplitude = 0f;
        private float spreadBloomValuePerSecond = 3f;

        private float maxChargeTime;
        private float tickDamageStopwatch;
        private float minimumDuration;

        private Vector3 currentAimVector;

        private GameObject beamEffectInstance;
        private BeamFromPoints beamEffectComponent;
        private float turnSpeedAngle;
        private float range;

        private bool canceled = true;
        private Transform muzzleTransform;
        private AimAnimator aimAnimator;
        private AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;
        public override void OnEnter()
        {
            base.OnEnter();
            this.minimumDuration = baseMinimumDuration / this.attackSpeedStat;
            //this.maxChargeTime = baseMaxChargeTime / this.attackSpeedStat;
            this.currentAimVector = base.inputBank.aimDirection;
            this.turnSpeedAngle = minTurnAnglePerSecond;
            //this.range = minRange;
            //muzzle
            muzzleTransform = base.FindModelChild("MuzzleLaser");       
            if(muzzleTransform)
            {
                this.beamEffectInstance = GameObject.Instantiate(beamEffectPrefab, muzzleTransform.position, Quaternion.identity);
                this.beamEffectComponent = beamEffectInstance.GetComponent<BeamFromPoints>();
            }
            

            base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedPenaltyCoefficient;

            var animator = base.GetModelAnimator();
            if (animator)
            {
                animator.SetLayerWeight(animator.GetLayerIndex("Body, Laser"), 1f);
                animator.SetBool("inLaser", true);
            }
            this.aimAnimator = base.GetAimAnimator();
            if(this.aimAnimator)
            {
                this.animatorDirectionOverrideRequest = aimAnimator.RequestDirectionOverride(new Func<Vector3>(this.GetAimDirection));
            }
        }

        public override void Update()
        {
            base.Update();

            Vector3 aimInput = base.inputBank.aimDirection;
            this.currentAimVector = Vector3.RotateTowards(this.currentAimVector, aimInput, Mathf.Deg2Rad * turnSpeedAngle * Time.deltaTime, 0);
            if (this.beamEffectComponent)
            {
                Vector3 point = this.currentAimVector * this.range + base.transform.position;
                if (Util.CharacterRaycast(base.gameObject, new Ray(base.inputBank.aimOrigin, this.currentAimVector), out RaycastHit raycastHit, this.range, LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
                {
                    point = raycastHit.point;
                }
                //beam is shitcode 
                this.beamEffectComponent.startPoint.position = muzzleTransform.position; 
                this.beamEffectComponent.endPoint.position = point;
            }
        }

        private Vector3 GetAimDirection()
        {
            return this.currentAimVector;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.SetAimTimer(2f);
            float t = base.fixedAge / this.maxChargeTime;
            float damageCoefficient = maxDamageCoefficientPerSecond / baseTicksPerSecond;// Mathf.Lerp(minDamageCoefficientPerSecond, maxDamageCoefficientPerSecond, t) * (1 / baseTicksPerSecond);
            this.turnSpeedAngle = minTurnAnglePerSecond;// Mathf.Lerp(maxTurnAnglePerSecond, minTurnAnglePerSecond * base.characterBody.attackSpeed, t);
            this.range = maxRange;// Mathf.Lerp(minRange, maxRange, t);

            this.tickDamageStopwatch -= Time.fixedDeltaTime;

            base.characterBody.AddSpreadBloom(this.spreadBloomValuePerSecond * (1 / baseTicksPerSecond));
            if (this.tickDamageStopwatch <= 0f)
            {
                this.tickDamageStopwatch += 1 / baseTicksPerSecond;
                this.FireBullet(damageCoefficient, this.currentAimVector);
            }

            if (((base.fixedAge >= this.minimumDuration && !base.inputBank.skill1.down) || base.characterBody.isSprinting) && base.isAuthority)
            {
                canceled = false;
                this.outer.SetNextState(new ExitLastPrism());
                return;
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if(this.animatorDirectionOverrideRequest != null)
            {
                this.animatorDirectionOverrideRequest.Dispose();
            }
            var animator = base.GetModelAnimator();
            if (animator)
            {
                animator.SetLayerWeight(animator.GetLayerIndex("Body, Laser"), 0f);
                animator.SetBool("inLaser", false);
            }
            if (canceled)
            {
                base.PlayAnimation("FullBody, Override", "BufferEmpty");
                base.PlayAnimation("Gesture, Override", "BufferEmpty");
            }
            if (this.beamEffectInstance)
            {
                Destroy(this.beamEffectInstance);
            }

            base.characterMotor.walkSpeedPenaltyCoefficient = 1;
        }

        private void FireBullet(float damageCoefficient, Vector3 direction)
        {
            Ray aimRay = base.GetAimRay();
            //base.AddRecoil(-1f * this.recoilAmplitude, -2f * this.recoilAmplitude, -0.5f * this.recoilAmplitude, 0.5f * this.recoilAmplitude);
            //if (this.muzzleflashEffectPrefab)
            //{
            //    EffectManager.SimpleMuzzleFlash(this.muzzleflashEffectPrefab, base.gameObject, this.muzzle, false);
            //}
            if (base.isAuthority)
            {
                DamageTypeCombo damageType = DamageType.Generic;
                damageType.damageSource = DamageSource.Primary;
                new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = direction,
                    muzzleName = "", /////////
                    maxDistance = this.range,
                    minSpread = 0f,
                    maxSpread = 0f,
                    radius = LastPrism.bulletRadius,
                    falloffModel = BulletAttack.FalloffModel.None,
                    smartCollision = true,
                    stopperMask = LayerIndex.world.mask,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    damage = damageCoefficient * this.damageStat,
                    procCoefficient = LastPrism.procCoefficientPerSecond / baseTicksPerSecond,
                    force = LastPrism.forcePerSecond / baseTicksPerSecond,
                    isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
                    damageType = damageType,
                    hitEffectPrefab = null, //this.hitEffectPrefab
                }.Fire();
            }
            
        }

        private void PlayAnimation(string animationStateName, string playbackRateParam, float duration)
        {
            string layerName = base.isGrounded ? "FullBody, Override" : "Gesture, Override";
            base.PlayAnimation(layerName, animationStateName, playbackRateParam, duration);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
