using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using Moonstorm.Starstorm2.Components;
namespace EntityStates.Cyborg2
{
    public class LastPrism : BaseSkillState
    {
        public static float minDamageCoefficientPerSecond = 2f;
        public static float maxDamageCoefficientPerSecond = 10f;
        public static float procCoefficientPerSecond = 0.7f;
        public static float forcePerSecond = 500f;
        public static float baseTicksPerSecond = 6f;

        public static float baseMaxChargeTime = 2f;
        public static float baseMinimumDuration  = 0.25f;

        public static float minRange = 8f;
        public static float maxRange = 50f;
        public static float bulletRadius = 1.5f;

        public static float maxTurnAnglePerSecond = 720f;
        public static float minTurnAnglePerSecond = 90f;

        public static GameObject beamEffectPrefab;


        public static float walkSpeedPenaltyCoefficient = 0.33f;

        public float recoilAmplitude = 0f;
        public float spreadBloomValuePerSecond = 3f;

        private float maxChargeTime;
        private float tickDamageStopwatch;
        private float minimumDuration;

        private Vector3 currentAimVector;

        private GameObject beamEffectInstance;
        private BeamFromPoints beamEffectComponent;
        private float turnSpeedAngle;
        private float range;

        public override void OnEnter()
        {
            base.OnEnter();
            this.minimumDuration = baseMinimumDuration / this.attackSpeedStat;
            this.maxChargeTime = baseMaxChargeTime / this.attackSpeedStat;
            this.currentAimVector = base.inputBank.aimDirection;
            this.turnSpeedAngle = minTurnAnglePerSecond;
            this.range = minRange;
            //muzzle
            this.beamEffectInstance = GameObject.Instantiate(beamEffectPrefab, base.characterBody.corePosition, Quaternion.identity);
            this.beamEffectComponent = beamEffectInstance.GetComponent<BeamFromPoints>();

            base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedPenaltyCoefficient;
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
                this.beamEffectComponent.startPoint.position = base.characterBody.corePosition; // MUZZZZZLLLLLLE
                this.beamEffectComponent.endPoint.position = point;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            float t = base.fixedAge / this.maxChargeTime;
            float damageCoefficient = Mathf.Lerp(minDamageCoefficientPerSecond, maxDamageCoefficientPerSecond, t) * (1 / baseTicksPerSecond);
            this.turnSpeedAngle = Mathf.Lerp(maxTurnAnglePerSecond, minTurnAnglePerSecond * base.characterBody.attackSpeed, t);
            this.range = Mathf.Lerp(minRange, maxRange, t);

            this.tickDamageStopwatch -= Time.fixedDeltaTime;

            base.characterBody.AddSpreadBloom(this.spreadBloomValuePerSecond * (1 / baseTicksPerSecond));
            if (this.tickDamageStopwatch <= 0f)
            {
                this.tickDamageStopwatch += 1 / baseTicksPerSecond;
                this.FireBullet(damageCoefficient, this.currentAimVector);
            }

            if (((base.fixedAge >= this.minimumDuration && !base.IsKeyDownAuthority()) || base.characterBody.isSprinting) && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if(this.beamEffectInstance)
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
                    hitEffectPrefab = null, //this.hitEffectPrefab
                }.Fire();
            }
            
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
