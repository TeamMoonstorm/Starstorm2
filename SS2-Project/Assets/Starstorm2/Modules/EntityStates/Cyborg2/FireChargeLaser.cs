using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
namespace EntityStates.Cyborg2
{
    public class FireChargeLaser : BaseSkillState
    {
        public static float selfKnockbackForce = 1500f;
        public static float walkSpeedCoefficient = 0.5f;

        public static float baseDuration = 0.5f;
        public static float damageCoefficient = 8f;
        public static float procCoefficient = 1f;
        public static float force = 500f;
        public static float bulletRadius = 4.5f;
        public static float maxDistance = 999f;
        public static GameObject hitEffectPrefab;

        public static int numTicks = 2;
        public static float tickDamageCoefficient = 1.5f;
        public static float tickProcCoefficient = 0.5f;
        public static float tickRadius = 1f;
        public static GameObject tickHitEffectPrefab;    
               
        public static GameObject laserEffectPrefab;
        public static string muzzleName = "CannonR";

        private bool crit;
        private float tickDamageStopwatch;
        private float tickDamageTime;
        private int ticksFired;
        private float duration;
        private bool hasFired;
        private GameObject laserEffectInstance;
        private Transform laserEffectEndTransform;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            this.tickDamageTime = this.duration / numTicks;
            this.crit = base.RollCrit();

            base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;

            Transform muzzle = base.FindModelChild(muzzleName);
            if(muzzle && laserEffectPrefab)
            {
                this.laserEffectInstance = GameObject.Instantiate(laserEffectPrefab, muzzle);

            }
            
        }

        private void FireTickDamage()
        {
            Ray aimRay = base.GetAimRay();
            new BulletAttack
            {
                aimVector = aimRay.direction,
                origin = aimRay.origin,
                owner = base.gameObject,
                weapon = base.gameObject,
                damage = this.damageStat * tickDamageCoefficient,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.None,
                stopperMask = LayerIndex.noCollision.collisionMask,
                procChainMask = default(ProcChainMask),
                procCoefficient = tickProcCoefficient,
                maxDistance = maxDistance,
                radius = tickRadius,
                isCrit = this.crit,
                muzzleName = muzzleName,
                hitEffectPrefab = tickHitEffectPrefab,
                smartCollision = true,
            }.Fire();
        }

        private void FireLaser()
        {          
            Ray aimRay = base.GetAimRay();

            if (!base.isGrounded && base.isAuthority && base.characterMotor)
            {
                Vector3 direction = aimRay.direction * -1f;
                base.characterMotor.ApplyForce(direction * FireChargeLaser.selfKnockbackForce, true);
            }

            new BulletAttack
            {
                aimVector = aimRay.direction,
                origin = aimRay.origin,
                owner = base.gameObject,
                weapon = base.gameObject,
                damage = this.damageStat * damageCoefficient,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.None,
                stopperMask = LayerIndex.noCollision.collisionMask,
                force = force,
                procChainMask = default(ProcChainMask),
                procCoefficient = procCoefficient,
                maxDistance = maxDistance,
                radius = bulletRadius,
                isCrit = base.RollCrit(),
                muzzleName = muzzleName,
                hitEffectPrefab = hitEffectPrefab,
                smartCollision = true,
            };
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterMotor.velocity.y = Mathf.Max(base.characterMotor.velocity.y, 0);

            if(this.tickDamageStopwatch <= 0 && ticksFired < numTicks)
            {
                this.ticksFired++;
                this.tickDamageStopwatch += this.tickDamageTime;
                this.FireTickDamage();
            }

            if(base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if(!this.hasFired)
                this.FireLaser();

            base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
