using RoR2;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R2API.ScriptableObjects;
using MSU;
using RoR2.Projectile;
using System.Collections;
using R2API;

namespace EntityStates.Nuke.Weapon
{
    public class FireFart : BaseNukeFireState
    {
        public static SerializableDamageColor damageColor;
        public static float extraDamageCoefficient;
        public static float baseForce;
        public static float baseRadius;
        public static float procCoefficient;
        public static float baseUpwardVelocity;
        public static float maxUpwardVelocity;
        public static Vector3 bonusForce;
        public static GameObject poolProjectile;
        public static GameObject poolDOT;

        private BlastAttack _blastAttack;
        public override void OnEnter()
        {
            bool crit = RollCrit();
            base.OnEnter();
            if (isAuthority)
            {
                _blastAttack = new BlastAttack
                {
                    attacker = gameObject,
                    inflictor = gameObject,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    baseDamage = damageStat * charge * extraDamageCoefficient,
                    baseForce = baseForce * charge,
                    radius = baseRadius * charge,
                    bonusForce = bonusForce * charge,
                    procCoefficient = procCoefficient,
                    teamIndex = teamComponent.AsValidOrNull()?.teamIndex ?? TeamIndex.None,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    damageType = DamageType.Stun1s,
                    position = characterBody.footPosition,
                    canRejectForce = false,
                    crit = crit,
                    damageColorIndex = damageColor.DamageColorIndex,
                };
                _blastAttack.AddModdedDamageType(SS2.Survivors.Nuke.NuclearDamageType);
                _blastAttack.Fire();

                if (!isGrounded)
                {
                    Vector3 velocity = characterMotor.velocity;
                    float yVelocity = baseUpwardVelocity * charge;
                    velocity.y = Mathf.Min(yVelocity, maxUpwardVelocity);
                    characterMotor.velocity = velocity;
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        crit = crit,
                        damage = damageStat * charge * extraDamageCoefficient,
                        damageColorIndex = damageColor.DamageColorIndex,
                        owner = gameObject,
                        position = characterBody.footPosition,
                        rotation = Util.QuaternionSafeLookRotation(Vector3.down),
                        projectilePrefab = poolProjectile,
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
                else
                {
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        crit = crit,
                        damage = damageStat * charge * extraDamageCoefficient,
                        damageColorIndex = damageColor.DamageColorIndex,
                        owner = gameObject,
                        position = characterBody.footPosition,
                        rotation = Quaternion.identity,
                        projectilePrefab = poolDOT,
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
        }
    }
}
