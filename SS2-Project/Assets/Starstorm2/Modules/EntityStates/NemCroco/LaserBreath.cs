using R2API;
using RoR2;
using RoR2.Projectile;
using SS2;
using SS2.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.UI;

namespace EntityStates.NemCroco
{
    public class ChargeLaserBreath : BaseSkillState
    {
        private static float baseDuration = 0.3f;
        public static GameObject effectPrefab;
        private static string enterSoundString = "";
        private static string muzzleString = "MouthMuzzle";
        private float duration;
        private GameObject effectInstance;
        protected EffectManagerHelper _efhChargeEffect = null;

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            StartAimMode();
            duration = baseDuration / attackSpeedStat;

            Util.PlayAttackSpeedSound(enterSoundString, gameObject, attackSpeedStat);
            Transform muzzleTransform = FindModelChild(muzzleString);
            if (muzzleTransform)
            {
                if (!EffectManager.ShouldUsePooledEffect(effectPrefab))
                {
                    effectInstance = UnityEngine.Object.Instantiate<GameObject>(effectPrefab, muzzleTransform.position, muzzleTransform.rotation);
                }
                else
                {
                    _efhChargeEffect = EffectManager.GetAndActivatePooledEffect(effectPrefab, muzzleTransform.position, muzzleTransform.rotation);
                    effectInstance = _efhChargeEffect.gameObject;
                }
                effectInstance.transform.parent = muzzleTransform;
                effectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
            }

            //PlayAnimation("Gesture, Mouth", "ChargeLaserBreath", "ChargeLaserBreath.playbackRate", duration);
            PlayAnimation("Gesture, Override", "ChargeLaserBreath", "LaserBreath.playbackRate", duration);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (effectInstance)
            {
                if (!EffectManager.UsePools)
                {
                    EntityState.Destroy(effectInstance);
                }
                else
                {
                    if (_efhChargeEffect != null && _efhChargeEffect.OwningPool != null)
                    {
                        _efhChargeEffect.OwningPool.ReturnObject(_efhChargeEffect);
                    }
                    else
                    {
                        EntityState.Destroy(effectInstance);
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration)
            {
                if (isAuthority)
                {
                    outer.SetNextState(new FireLaserBreath());
                }
            }
        }
    }

    public class FireLaserBreath : BaseState
    {
        private static float baseDuration = .66f;
        private static float ticksPerSecond = 6f;
        private static float procCoefficientPerSecond = 4f;
        private static float damageCoefficientPerSecond = 3f;
        private static float spreadBloomPerSecond = 1.2f;
        private static float bulletRadius = 1.5f;
        private static float range = 48f;
        private static float force = 125f;
        private static float recoilForce = 30f;
        private static float turnSpeed = 360f;
        private static float recoil = 0.0f;
        private static float walkSpeedCoefficient = 0.6f;
        public static GameObject impactEffectPrefab;

        private static float exitDamageCoefficient = 1.5f;
        private static float exitProcCoefficient = 1f;
        private static float exitRecoil = 2f;
        private static float exitRecoilForce = 100f;
        private static float exitForce = 150f;
        private static float exitSpreadBloom = 0.4f;
        public static GameObject exitMuzzleEffectPrefab;
        public static GameObject exitTracerPrefab;

        private static string muzzleString = "MouthMuzzle";
        private static string enterSoundString = "";
        private static string startLoopSoundString = "Play_mage_R_start";
        private static string endLoopSoundString = "Play_mage_R_end";
        private static string exitSoundString = "Play_acrid_m2_shoot";
        public static GameObject muzzleEffectPrefab;
        public static GameObject beamEffectPrefab;
        private static float beamEffectDistance = 32f;
        public static GameObject crosshairPrefab;

        private Transform flamethrowerTransform;
        private Transform beamTransform;
        private Transform beamEndTransform;
        private AimAnimator aimAnimator;
        private AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;
        private Vector3 currentAimVector;
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        private ChildLocator childLocator;
        private bool isCrit;
        private float duration;
        private float baseTickRate;
        private float tickRate;
        private float stopwatch;

        private Transform muzzleTransform;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            
            duration = baseDuration / attackSpeedStat;
            baseTickRate = 1f / ticksPerSecond;
            tickRate = baseTickRate / attackSpeedStat;
            currentAimVector = inputBank.aimDirection;
            stopwatch = tickRate;

            characterBody.SetAimTimer(2f);
            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;

            Util.PlaySound(enterSoundString, gameObject);
            Util.PlaySound(startLoopSoundString, gameObject);
            //PlayCrossfade("Gesture, Mouth", "FireLaserBreath", 0.1f);
            PlayCrossfade("Gesture, Override", "FireLaserBreath", 0.1f);
            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleString, false);
            }

            if (beamEffectPrefab)
            {
                muzzleTransform = FindModelChild(muzzleString);
                if (muzzleTransform)
                {
                    ////////// TODO: PARENT THIS TO HEAD INSTEAD OF UPDATING POSITION
                    beamTransform = GameObject.Instantiate(beamEffectPrefab, muzzleTransform.transform.position, Quaternion.identity).transform;
                    beamEndTransform = beamTransform.Find("End");
                }
            }

            aimAnimator = GetAimAnimator();
            if (aimAnimator)
            {
                animatorDirectionOverrideRequest = aimAnimator.RequestDirectionOverride(new Func<Vector3>(GetAimDirection));
            }

            if (crosshairPrefab)
            {
                crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(characterBody, crosshairPrefab, CrosshairUtils.OverridePriority.Skill);
            }


            if (isAuthority)
            {
                isCrit = RollCrit();
            }
        }
        private Vector3 GetAimDirection()
        {
            return currentAimVector;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.SetAimTimer(2f);
            characterBody.AddSpreadBloom(spreadBloomPerSecond * Time.fixedDeltaTime);

            stopwatch += Time.fixedDeltaTime;
            int attacksThisFrame = 0;
            while (stopwatch > tickRate && attacksThisFrame <= 3) //if youre firing 3 projectiles per frame, youve got bigger problems than a dps loss
            {
                attacksThisFrame++;
                stopwatch -= tickRate;
                Fire();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void Update()
        {
            base.Update();

            currentAimVector = Vector3.RotateTowards(currentAimVector, inputBank.aimDirection, Mathf.Deg2Rad * turnSpeed * Time.deltaTime, 0);

            if (beamTransform && muzzleTransform)
            {
                beamTransform.position = muzzleTransform.position;
            }
            if (beamEndTransform)
            {
                Ray ray = GetAimRay();
                ray.direction = currentAimVector;
                beamEndTransform.position = ray.GetPoint(beamEffectDistance);
            }
            else if (beamTransform)
            {
                beamTransform.position = muzzleTransform.position;
                beamTransform.transform.forward = currentAimVector;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            characterMotor.walkSpeedPenaltyCoefficient = 1f;

            Util.PlaySound(endLoopSoundString, gameObject);
            Util.PlaySound(exitSoundString, gameObject);

            if (!outer.destroying)
            {
                FireExitBurst();
            }

            if (flamethrowerTransform)
            {
                Destroy(flamethrowerTransform.gameObject);
            }
            if (beamTransform)
            {
                Destroy(beamTransform.gameObject);
            }
            if (animatorDirectionOverrideRequest != null)
            {
                animatorDirectionOverrideRequest.Dispose();
            }
            if (crosshairOverrideRequest != null)
            {
                crosshairOverrideRequest.Dispose();
            }
        }

        private void Fire()
        {
            characterBody.SetAimTimer(2f);
            AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

            Ray aimRay = GetAimRay();
            aimRay.direction = currentAimVector;
            if (isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = gameObject;
                bulletAttack.weapon = gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.damage = damageCoefficientPerSecond * baseTickRate * damageStat;
                bulletAttack.force = force;
                bulletAttack.muzzleName = muzzleString;
                bulletAttack.hitEffectPrefab = impactEffectPrefab;
                bulletAttack.isCrit = isCrit;
                bulletAttack.radius = bulletRadius;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.stopperMask = 0;
                bulletAttack.procCoefficient = procCoefficientPerSecond * baseTickRate;
                bulletAttack.maxDistance = range;
                bulletAttack.smartCollision = true;
                bulletAttack.damageType = DamageTypeCombo.GenericSecondary;
                bulletAttack.damageType.AddModdedDamageType(SS2.Survivors.NemCroco.RadiationOnHit);
                bulletAttack.Fire();
                if (characterMotor)
                {
                    characterMotor.ApplyForce(aimRay.direction * -recoilForce, false, false);
                }
            }
        }

        private void FireExitBurst()
        {
            PlayCrossfade("Gesture, Mouth", "FireLaserBurst", 0.1f);
            AddRecoil(-0.4f * exitRecoil, -0.8f * exitRecoil, -0.3f * exitRecoil, 0.3f * exitRecoil);
            characterBody.AddSpreadBloom(exitSpreadBloom);
            if (exitMuzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(exitMuzzleEffectPrefab, gameObject, muzzleString, false);
            }

            Ray aimRay = GetAimRay();
            aimRay.direction = currentAimVector;
            if (isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = gameObject;
                bulletAttack.weapon = gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.damage = exitDamageCoefficient * damageStat;
                bulletAttack.force = exitForce;
                bulletAttack.muzzleName = muzzleString;
                bulletAttack.hitEffectPrefab = impactEffectPrefab;
                bulletAttack.isCrit = isCrit;
                bulletAttack.radius = bulletRadius;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.stopperMask = 0;
                bulletAttack.procCoefficient = exitProcCoefficient;
                bulletAttack.maxDistance = range;
                bulletAttack.smartCollision = true;
                bulletAttack.damageType = DamageTypeCombo.GenericSecondary;
                bulletAttack.damageType.AddModdedDamageType(SS2.Survivors.NemCroco.RadiationOnHit);

                bulletAttack.tracerEffectPrefab = exitTracerPrefab;

                bulletAttack.Fire();
                if (characterMotor)
                {
                    characterMotor.ApplyForce(aimRay.direction * -exitRecoilForce, false, false);
                }
            }
        }
    }
}
