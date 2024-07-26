using Moonstorm;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.Nuke.Weapon
{
    public class FireLaunch : BaseNukeWeaponFireState
    {
        [Header("Launch Parameters")]
        public static float minLaunchSpeed;
        public static float maxLaunchSpeed;
        public static float baseDuration;

        [Header("Explosion Parameters")]
        public static SerializableDamageColor damageColor;
        public static float baseForce;
        public static float baseRadius;
        public static float procCoefficient;
        public static Vector3 bonusForce;

        private float _launchSpeed;
        private Vector3 _velocity;
        private float _duration;
        public override void OnEnter()
        {
            base.OnEnter();
            if(isAuthority)
            {
                characterMotor.Motor.ForceUnground();
                _velocity = CalculateLaunchVelocity(characterMotor.velocity, GetAimRay().direction, Charge, minLaunchSpeed, maxLaunchSpeed);
                characterMotor.velocity = _velocity;
                characterDirection.forward = _velocity.normalized;
                _launchSpeed = _velocity.magnitude;
                _duration = baseDuration / attackSpeedStat;

                BlastAttack attack = new BlastAttack
                {
                    attacker = gameObject,
                    baseDamage = damageStat * Charge,
                    baseForce = baseForce * Charge,
                    radius = baseRadius * Charge,
                    bonusForce = bonusForce * Charge,
                    procCoefficient = procCoefficient,
                    teamIndex = teamComponent.AsValidOrNull()?.teamIndex ?? TeamIndex.None,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    position = characterBody.footPosition,
                    canRejectForce = false,
                    inflictor = gameObject,
                    crit = RollCrit(),
                    damageColorIndex = damageColor.DamageColorIndex
                };
                attack.AddModdedDamageType(Moonstorm.Starstorm2.DamageTypes.Nuclear.NuclearDamageType);
                attack.Fire();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(isAuthority)
            {
                characterBody.isSprinting = true;
            }
            if (fixedAge > _duration)
                outer.SetNextStateToMain();
        }

        private static Vector3 CalculateLaunchVelocity(Vector3 currentVelocity, Vector3 aimDirection, float charge, float minLaunchSpeed, float maxLaunchSpeed)
        {
            currentVelocity = ((Vector3.Dot(currentVelocity, aimDirection) < 0f) ? Vector3.zero : Vector3.Project(currentVelocity, aimDirection));
            return currentVelocity + aimDirection * Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, charge);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}