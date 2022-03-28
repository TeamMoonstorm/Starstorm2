/*using EntityStates;
using RoR2;
using UnityEngine;

namespace EntityStates.Nucleator
{
    class FireQuarantine : BaseSkillState
    {
        public static float minChargeForceCoef = 3F;
        public static float maxChargeForceCoef = 5F;
        public static float maxOverchargeDistanceCoef = 8F;
        public static float forceBase = 15F;
        private static float duration = 0.5f;

        public float charge;

        private float force;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();
            CalculateForce();

            PlayAnimation("Gesture, Override", "SecondaryRelease", "Secondary.playbackRate", duration);

            Ray aimRay = GetAimRay();

            EffectData effectData = new EffectData();
            effectData.origin = aimRay.origin + 2 * aimRay.direction;
            effectData.scale = 8;

            EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/FusionCellExplosion"), effectData, false);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration * 0.2f)
            {
                Shoot();
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        private void CalculateForce()
        {
            float chargeCoef;
            float forceCoef;
            float overchargeThreshold = NucleatorSkillStateBase.overchargeThreshold;

            if (charge < overchargeThreshold)
            {
                chargeCoef = charge / overchargeThreshold;
                forceCoef = chargeCoef * (maxChargeForceCoef - minChargeForceCoef) + minChargeForceCoef;
                force = forceCoef * forceBase;
            }
            else
            {
                chargeCoef = (charge - overchargeThreshold) / (1 - overchargeThreshold);
                forceCoef = chargeCoef * (maxOverchargeDistanceCoef - maxChargeForceCoef) + maxChargeForceCoef;
                force = forceCoef * forceBase;
            }
        }

        private void Shoot()
        {
            if (!hasFired)
            {
                hasFired = true;
                bool isCrit = RollCrit();

                Util.PlayAttackSpeedSound(EntityStates.Commando.CommandoWeapon.FirePistol2.firePistolSoundString, gameObject, attackSpeedStat);

                if (isAuthority)
                {
                    var aimRay = GetAimRay();
                    Collider[] colliders = Physics.OverlapSphere(characterBody.corePosition, 25f, LayerIndex.defaultLayer.mask);
                    foreach (Collider col in colliders)
                    {
                        Vector3 vectorToCollider = (col.transform.position - characterBody.corePosition).normalized;
                        if (Vector3.Dot(vectorToCollider, aimRay.direction) > 0.5)
                        {
                            var force = col.GetComponent<Rigidbody>().mass * this.force;
                            col.GetComponent<HealthComponent>().TakeDamage(new DamageInfo()
                            {
                                damage = damageStat * 3.5f,
                                attacker = gameObject,
                                crit = isCrit,
                                position = aimRay.origin,
                                force = vectorToCollider * force,
                                procCoefficient = 1f,
                                damageType = DamageType.Generic | DamageType.Stun1s,
                                damageColorIndex = DamageColorIndex.Default
                                //dotIndex = characterBody.HasBuff(Buffs.nucleatorSpecialBuff) ? NucleatorCore.radiationDotIndex : DotController.DotIndex.None
                            });
                        }
                    }
                }
            }
        }
    }
}

*/