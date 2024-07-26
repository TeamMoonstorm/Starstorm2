using Moonstorm;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.Nuke.Weapon
{
    public class FireCraterFart : BaseNukeWeaponFireState
    {
        public static SerializableDamageColor damageColor;
        public static float extraDamageCoefficient;
        public static float baseForce;
        public static float baseRadius;
        public static float procCoefficient;
        public static Vector3 bonusForce;
        public static float airControlDuringFall;
        public static float baseDownardsVelocity;
        public static float maxDownardsVelocity;
        public static GameObject poolDOT;

        private float originalAirControl;
        private bool isCrit;
        private bool detonateNextFrame;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            originalAirControl = characterMotor.airControl;
            characterMotor.airControl = airControlDuringFall;
            if(isAuthority)
            {
                isCrit = RollCrit();
                float yVelocity = -(baseDownardsVelocity * Charge);
                characterMotor.velocity.y = Mathf.Min(yVelocity, maxDownardsVelocity);
                characterMotor.onMovementHit += Fart;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(isAuthority)
            {
                characterMotor.moveDirection = inputBank.moveVector;
                if(detonateNextFrame || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround))
                {
                    var blastAttack = new BlastAttack
                    {
                        attacker = gameObject,
                        inflictor = gameObject,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        baseDamage = damageStat * extraDamageCoefficient * Charge,
                        baseForce = baseForce * Charge,
                        radius = baseRadius * Charge,
                        bonusForce = bonusForce * Charge,
                        procCoefficient = procCoefficient,
                        teamIndex = teamComponent.AsValidOrNull()?.teamIndex ?? TeamIndex.None,
                        falloffModel = BlastAttack.FalloffModel.SweetSpot,
                        position = characterBody.footPosition,
                        canRejectForce = false,
                        damageType = DamageType.Stun1s,
                        crit = isCrit,
                        damageColorIndex = damageColor.DamageColorIndex,
                    };
                    blastAttack.AddModdedDamageType(Moonstorm.Starstorm2.DamageTypes.Nuclear.NuclearDamageType);
                    blastAttack.Fire();
                    FireProjectileInfo info = new FireProjectileInfo
                    {
                        crit = isCrit,
                        damage = damageStat * extraDamageCoefficient * Charge,
                        damageColorIndex = damageColor.DamageColorIndex,
                        owner = gameObject,
                        position = characterBody.footPosition,
                        projectilePrefab = poolDOT,
                        rotation = Quaternion.identity,
                    };
                    ProjectileManager.instance.FireProjectile(info);
                    outer.SetNextStateToMain();
                }
            }
        }

        private void Fart(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            detonateNextFrame = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            characterMotor.airControl = originalAirControl;
            characterBody.bodyFlags ^= CharacterBody.BodyFlags.IgnoreFallDamage;
        }
    }
}