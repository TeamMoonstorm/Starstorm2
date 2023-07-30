using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemHuntress2
{
    public class BaseFireArrow : BaseSkillState
    {
        public float charge;
        public GameObject projectilePrefab;

        public float maxDamageCoefficient;
        public float minDamageCoefficient;
        public float procCoefficient;
        public float minRecoil;
        public float maxRecoil;
        public float minProjectileSpeed;
        public float maxProjectileSpeed;
        public float maxSpreadAngle;

        public float baseDuration;

        private float damageCoefficient;
        private float recoil;
        private float projectileSpeed;
        private float duration;
        private float spread;
        private bool hasFired;

        private NemHuntressController nhc;

        //private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        public override void OnEnter()
        {
            base.OnEnter();

            nhc = characterBody.GetComponent<NemHuntressController>();
            if (nhc != null)
            {
                projectilePrefab = nhc.currentArrow;
            }

            if (projectilePrefab == null)
            {
                Debug.Log("nemhuntress arrow projectile prefab is null .. what did you do?");
                return;
            }

            characterBody.SetAimTimer(2f);
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            damageCoefficient = Util.Remap(charge, 0f, 1f, minDamageCoefficient, maxDamageCoefficient);
            recoil = Util.Remap(charge, 0f, 1f, minRecoil, maxRecoil);
            projectileSpeed = Util.Remap(charge, 0f, 1f, minProjectileSpeed, maxProjectileSpeed);
            /*if (charge >= 0.8f)
                charge = 1f;*/
            spread = 0; //lol

            Util.PlayAttackSpeedSound("NemHuntressFireBow", gameObject, attackSpeedStat);

            FireProjectile();
        }

        public virtual void FireProjectile()
        {
            if (!hasFired)
            {
                hasFired = true;

                float damage = damageCoefficient * damageStat;
                AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                characterBody.AddSpreadBloom(0.33f * recoil);
                Ray aimRay = GetAimRay();

                Vector3 spreadDirection = CalculateSpreadDirection(aimRay.direction, spread, spread);
                FireArrowProjectile(aimRay.origin, spreadDirection, damage);

                //🙂
                if (skillLocator.primary.stock > 1)
                {
                    Vector3 leftSpreadDirection = CalculateSpreadDirection(aimRay.direction, spread + 0f, -2f - spread);
                    FireArrowProjectile(aimRay.origin, leftSpreadDirection, damage);

                    if (skillLocator.primary.stock > 2)
                    {
                        Vector3 rightSpreadDirection = CalculateSpreadDirection(aimRay.direction, spread + 0f, spread + 2f);
                        FireArrowProjectile(aimRay.origin, rightSpreadDirection, damage);

                        if (skillLocator.primary.stock > 3)
                        {
                            Vector3 upLeftSpreadDirection = CalculateSpreadDirection(aimRay.direction, 1.5f + spread, spread + 1.5f);
                            FireArrowProjectile(aimRay.origin, upLeftSpreadDirection, damage);

                            if (skillLocator.primary.stock > 4)
                            {
                                Vector3 upRightSpreadDirection = CalculateSpreadDirection(aimRay.direction, 1.5f + spread, -1.5f - spread);
                                FireArrowProjectile(aimRay.origin, upRightSpreadDirection, damage);

                                if (skillLocator.primary.stock > 5)
                                {
                                    Vector3 downLeftSpreadDirection = CalculateSpreadDirection(aimRay.direction, -1.5f - spread, spread + 1.5f);
                                    FireArrowProjectile(aimRay.origin, downLeftSpreadDirection, damage);

                                    if (skillLocator.primary.stock > 6)
                                    {
                                        Vector3 downRightSpreadDirection = CalculateSpreadDirection(aimRay.direction, -1.5f - spread, -1.5f - spread);
                                        FireArrowProjectile(aimRay.origin, downRightSpreadDirection, damage);
                                    }
                                }
                            }
                        }
                    }
                }

                skillLocator.primary.stock = 1;
                nhc.currentArrow = nhc.baseArrow;
            }
        }

        private void FireArrowProjectile(Vector3 origin, Vector3 direction, float damage)
        {
            ProjectileManager.instance.FireProjectile(
                projectilePrefab,
                origin,
                Util.QuaternionSafeLookRotation(direction),
                gameObject,
                damage,
                0f,
                RollCrit(),
                DamageColorIndex.Default,
                null,
                projectileSpeed);
        }

        private Vector3 CalculateSpreadDirection(Vector3 aimDirection, float angleOffsetX, float angleOffsetY)
        {
            Quaternion spreadRotation = Quaternion.Euler(-angleOffsetX, angleOffsetY, 0f);
            return spreadRotation * aimDirection;
        }

        public override void OnExit()
        {
            base.OnExit();
            skillLocator.primary.UnsetSkillOverride(skillLocator.primary, skillLocator.primary.skillDef, GenericSkill.SkillOverridePriority.Contextual);
            skillLocator.special.cooldownScale = 1;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
