using UnityEngine;
using RoR2;
using RoR2.UI;
using RoR2.Projectile;
using System;
using UnityEngine.Networking;

namespace EntityStates.Mage.Weapon
{
    public class PrepSpear : BaseState
    {
        public static GameObject areaIndicatorPrefab;
        public static GameObject originIndicatorPrefab;
        public static GameObject trajectoryIndicatorPrefab;
        public static GameObject projectilePrefab;
        public static GameObject muzzleflashEffect;
        public static GameObject fireEffectPrefab;
        public static GameObject goodCrosshairPrefab;
        public static GameObject badCrosshairPrefab;

        private static float baseDuration = 0.5f;
        private static float damageCoefficient = 3f;
        private static float force = 400f;
        private static float blastRadius = 5f;

        private static float projectileSpeed = 200f;
        private static float projectileLifetime = 0.6f;
        private static float projectileRadius = 3f;
        private static float projectileCollisionRadius = 0.15f;
        private static float maxDistance = 600f;
        private static float maxAngle = 76f;
       
        private static float bounceForce = 1300f;
        private static float bounceUpForce = 1000f;
        private static float selfBounceForce = 32f;
        private static float selfUpForce = 10f;

        private static string fireSoundString = "";
        private static string prepWallSoundString = "";

        private float duration;
        private float stopwatch;
        private bool goodPlacement;
        private GameObject areaIndicatorInstance;
        private GameObject originIndicatorInstance;
        private GameObject trajectoryIndicatorInstance;
        private Transform trajectoryEndTransform;
        private CapsuleCollider trajectoryIndicatorCollider;
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;
        private bool placementFailed;

        private static int PrepWallStateHash = Animator.StringToHash("PrepWall");
        private static int PrepWallParamHash = Animator.StringToHash("PrepWall.playbackRate");
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(duration + 2f);

            PlayAnimation("Gesture, Additive", PrepWallStateHash, PrepWallParamHash, duration);
            Util.PlaySound(prepWallSoundString, gameObject);

            areaIndicatorInstance = GameObject.Instantiate(areaIndicatorPrefab);
            PlaceOriginIndicator();
            UpdateAreaIndicator();


            // TODO: figure out what to do when failing to place the origin (when terrain is out of range)
            // Loader style range indicator?
            // Refresh cooldown?
            // Grace period to aim at terrain?
            // Snap to closest terrain to aim point?
            // Place directly underneath player?
            // All of the above?
            if (placementFailed)
            {
                outer.SetNextStateToMain();
                skillLocator.utility.stock++;
                return;
            }
        }

        private void PlaceOriginIndicator()
        {
            placementFailed = true;

            RaycastHit hit;
            float raycastDistance = maxDistance;

            Ray aimRay = GetAimRay();
            if (Physics.Raycast(aimRay, out hit, raycastDistance, LayerIndex.world.mask))
            {
                placementFailed = false;
                originIndicatorInstance = GameObject.Instantiate(originIndicatorPrefab);

                originIndicatorInstance.transform.position = hit.point;
                originIndicatorInstance.transform.forward = hit.normal;

                trajectoryIndicatorInstance = GameObject.Instantiate(trajectoryIndicatorPrefab);
                trajectoryIndicatorInstance.transform.position = originIndicatorInstance.transform.position;
                trajectoryIndicatorInstance.transform.forward = originIndicatorInstance.transform.forward;
                trajectoryEndTransform = trajectoryIndicatorInstance.transform.Find("BeamEnd");
                if (trajectoryEndTransform)
                {
                    trajectoryEndTransform.position = originIndicatorInstance.transform.position;
                }
                trajectoryIndicatorCollider = trajectoryIndicatorInstance.GetComponentInChildren<CapsuleCollider>();
                if (trajectoryIndicatorCollider)
                {
                    trajectoryIndicatorCollider.radius = projectileRadius;
                    if (trajectoryIndicatorCollider.TryGetComponent(out UnseenHandIndicator indicator))
                    {
                        indicator.teamMask = TeamMask.GetUnprotectedTeams(characterBody.teamComponent.teamIndex);
                    }
                }
            }
        }
        private void UpdateAreaIndicator()
        {
            bool wasGoodPlacement = goodPlacement;
            goodPlacement = false;

            areaIndicatorInstance.SetActive(true);
            if (areaIndicatorInstance && originIndicatorInstance)
            {
                RaycastHit hit;
                Ray aimRay = GetAimRay();
                float projectileMaxDistance = projectileSpeed * projectileLifetime;
                Vector3 targetPosition = aimRay.GetPoint(projectileMaxDistance);
                if (Physics.Raycast(aimRay, out hit, projectileMaxDistance, LayerIndex.world.mask))
                {
                    targetPosition = hit.point;
                }

                Vector3 current = originIndicatorInstance.transform.forward;
                Vector3 target = targetPosition - originIndicatorInstance.transform.position;
                if (Vector3.Angle(current, target.normalized) < maxAngle)
                {
                    goodPlacement = true;
                }
                Vector3 projectileDirection = Vector3.RotateTowards(current, target, maxAngle * Mathf.Deg2Rad, 0f);

                Ray projectileRay = new Ray(originIndicatorInstance.transform.position, projectileDirection);
                Vector3 projectileHitPosition = projectileRay.GetPoint(projectileMaxDistance);
                if (Physics.SphereCast(projectileRay, radius: projectileCollisionRadius, out hit, projectileMaxDistance, LayerIndex.world.mask))
                {
                    projectileHitPosition = hit.point;
                    areaIndicatorInstance.transform.position = projectileHitPosition;
                    areaIndicatorInstance.transform.forward = -projectileRay.direction;
                }
                else
                {
                    areaIndicatorInstance.transform.position = projectileHitPosition;
                    areaIndicatorInstance.transform.rotation = Quaternion.identity;
                }

                if (trajectoryIndicatorInstance)
                {
                    trajectoryIndicatorInstance.transform.forward = projectileRay.direction;
                    if (trajectoryEndTransform)
                    {
                        trajectoryEndTransform.position = projectileHitPosition;
                    }
                    if (trajectoryIndicatorCollider)
                    {
                        float distance = (areaIndicatorInstance.transform.position - originIndicatorInstance.transform.position).magnitude;
                        trajectoryIndicatorCollider.transform.forward = projectileRay.direction;
                        trajectoryIndicatorCollider.height = distance;
                        trajectoryIndicatorCollider.center = new Vector3(0f, 0f, distance / 2f);
                    }
                }

                // NEED WARNING COLOR FOR INDICATOR!!
                if ((wasGoodPlacement != goodPlacement) || (crosshairOverrideRequest == null))
                {
                    crosshairOverrideRequest?.Dispose();

                    var crosshairPrefab = goodPlacement ? goodCrosshairPrefab : badCrosshairPrefab;
                    if (crosshairPrefab)
                    {
                        crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(characterBody, crosshairPrefab, CrosshairUtils.OverridePriority.Skill);
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateAreaIndicator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += GetDeltaTime();
            if ((stopwatch >= duration && !inputBank.skill3.down) && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void Fire()
        {
            Vector3 direction = areaIndicatorInstance.transform.position - originIndicatorInstance.transform.position;

            PlayAnimation("Gesture, Additive", "FireWall");
            Util.PlaySound(fireSoundString, gameObject);
            if (muzzleflashEffect)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffect, gameObject, "MuzzleLeft", false);
                EffectManager.SimpleMuzzleFlash(muzzleflashEffect, gameObject, "MuzzleRight", false);
            }
            if (fireEffectPrefab)
            {
                EffectManager.SimpleEffect(fireEffectPrefab, originIndicatorInstance.transform.position, Util.QuaternionSafeLookRotation(direction), false);
            }


            if (isAuthority)
            {
                var projectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = projectilePrefab,
                    position = originIndicatorInstance.transform.position,
                    rotation = Util.QuaternionSafeLookRotation(direction),
                    owner = gameObject,
                    damage = damageStat * damageCoefficient,
                    force = force,
                    crit = RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    fuseOverride = projectileLifetime,
                    speedOverride = projectileSpeed,
                };
                ProjectileManager.instance.FireProjectile(projectileInfo);

                Vector3 between = originIndicatorInstance.transform.position - characterBody.footPosition;
                if (between.sqrMagnitude <= blastRadius * blastRadius)
                {
                    PhysForceInfo info = new PhysForceInfo();
                    info.massIsOne = true;
                    info.resetVelocity = true;
                    info.ignoreGroundStick = true;
                    info.respectKnockupImmune = true;
                    info.force = direction.normalized * selfBounceForce;
                    info.force += Vector3.up * selfUpForce;
                    characterMotor.ApplyForceImpulse(info);
                }
            }

            if (NetworkServer.active)
            {
                SphereSearch search = new SphereSearch();
                search.radius = blastRadius;
                search.origin = originIndicatorInstance.transform.position;
                search.mask = LayerIndex.entityPrecise.mask;
                search = search.RefreshCandidates();
                foreach (HurtBox hurtBox in search.FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes())
                {
                    HealthComponent healthComponent = hurtBox.healthComponent;
                    if (healthComponent && healthComponent.gameObject != gameObject)
                    {
                        PhysForceInfo info = new PhysForceInfo();
                        //info.simplifiedMass = true;
                        info.resetVelocity = true;
                        info.ignoreGroundStick = true;
                        info.respectKnockupImmune = true;
                        info.disableAirControlUntilCollision = true;
                        info.force = bounceUpForce * Vector3.up + bounceForce * direction.normalized;
                        healthComponent.TakeDamageForce(info);
                    }
                }
            }

        }
        public override void OnExit()
        {
            if (!outer.destroying)
            {
                Fire();
            }

            if (areaIndicatorInstance)
            {
                Destroy(areaIndicatorInstance.gameObject);
            }
            if (originIndicatorInstance)
            {
                Destroy(originIndicatorInstance);
            }
            if (trajectoryIndicatorInstance)
            {
                Destroy(trajectoryIndicatorInstance);
            }

            crosshairOverrideRequest?.Dispose();

            base.OnExit();
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
