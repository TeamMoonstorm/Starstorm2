/*using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Nucleator
{
    class FireIrradiate : BaseSkillState
    {
        public static float minChargeDamageCoef = 1F;
        public static float maxChargeDamageCoef = 5F;
        public static float maxOverchargeDamageCoef = 9F;

        public float charge;

        private float damage;
        private float duration = 0.3f;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            CalculateDamage();
            Shoot();

            if (charge > NucleatorSkillStateBase.overchargeThreshold)
            {
                PlayAnimation("Gesture, Override", "PrimaryBig", "Primary.playbackRate", duration);
            }
            else
            {
                PlayAnimation("Gesture, Override", "PrimaryLight", "Primary.playbackRate", duration);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        private void CalculateDamage()
        {
            float chargeCoef;
            float damageCoef;
            float overchargeThreshold = NucleatorSkillStateBase.overchargeThreshold;

            if (charge < overchargeThreshold)
            {
                chargeCoef = charge / overchargeThreshold;
                damageCoef = chargeCoef * (maxChargeDamageCoef - minChargeDamageCoef) + minChargeDamageCoef;
                damage = damageStat * damageCoef;
            }
            else
            {
                chargeCoef = (charge - overchargeThreshold) / (1 - overchargeThreshold);
                damageCoef = chargeCoef * (maxOverchargeDamageCoef - maxChargeDamageCoef) + maxChargeDamageCoef;
                damage = damageStat * damageCoef;
            }
        }

        private void Shoot()
        {
            if (!hasFired)
            {
                hasFired = true;
                bool isCrit = RollCrit();

                Util.PlayAttackSpeedSound(EntityStates.Commando.CommandoWeapon.FirePistol2.firePistolSoundString, gameObject, attackSpeedStat);
                characterBody.AddSpreadBloom(EntityStates.Commando.CommandoWeapon.FirePistol2.spreadBloomValue * 1.5f);
                Ray aimRay = GetAimRay();
                StartAimMode(aimRay, 1f, false);
                if (isAuthority)
                {
                    var baseRadius = charge * 10;
                    GameObject nucleatorProjectile = Survivors.Nucleator.primaryProjectile;
                    nucleatorProjectile.GetComponent<NucleatorProjectile>().baseRadius = baseRadius;
                    nucleatorProjectile.GetComponent<NucleatorProjectile>().charge = charge;

                    nucleatorProjectile.GetComponent<ProjectileDamage>().damage = damage;

                    if (charge > NucleatorSkillStateBase.overchargeThreshold)
                    {
                        nucleatorProjectile.GetComponent<ProjectileDirectionalTargetFinder>().enabled = true;
                        nucleatorProjectile.GetComponent<ProjectileSteerTowardTarget>().enabled = true;
                    }
                    else
                    {
                        nucleatorProjectile.GetComponent<ProjectileDirectionalTargetFinder>().enabled = false;
                        nucleatorProjectile.GetComponent<ProjectileSteerTowardTarget>().enabled = false;
                    }

                    FireProjectileInfo info = new FireProjectileInfo()
                    {
                        crit = RollCrit(),
                        damage = 0f,
                        damageColorIndex = DamageColorIndex.Default,
                        damageTypeOverride = DamageType.Generic,
                        force = 0f,
                        owner = gameObject,
                        position = aimRay.origin + aimRay.direction.normalized,
                        procChainMask = default,
                        projectilePrefab = nucleatorProjectile,
                        rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                        target = null,
                        useFuseOverride = false,
                        useSpeedOverride = false
                    };
                    ProjectileManager.instance.FireProjectile(info);
                }
            }
        }
    }
}*/