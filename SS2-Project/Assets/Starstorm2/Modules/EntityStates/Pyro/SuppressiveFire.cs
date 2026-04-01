using RoR2;
using RoR2.Projectile;
using RoR2.UI;
using SS2;
using SS2.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Pyro
{
    public class SuppressiveFire : BaseSkillState
    {
        private static float baseFireInterval = 0.07f;
        public static float baseEntryDuration;
        public static float pressureDuration;

        private static float minProjectileSpeed = 30f;
        private static float maxProjectileSpeed = 120f;
        private static float durationForMaxSpeed = 0.5f;

        private float fireInterval;
        private float stopwatch;
        private float heatStopwatch;
        private float flamethrowerStopwatch;
        private float entryDuration;

        private bool hasBegunFlamethrower;

        private static float heatCostPerSecond = 20f;
        public static float tickProcCoefficient;
        private static float damagePerSecond = 4.51f;
        public static float recoilForce;
        public static float force;
        public static float radius;
        public static float recoil;
        private static float spreadBloomPerSecond = 1f;
        public static string muzzleString;

        public static GameObject projectilePrefab;
        public static GameObject igniteProjectilePrefab;
        public static GameObject flameEffectPrefab;
        public static GameObject crosshairPrefab;

        private static string startFireSoundLoopString = "Play_pyro_primary_loop";
        private static string stopFireSoundLoopString = "Stop_pyro_primary_loop";
        private static string exitSoundString = "Play_pyro_primary_end";

        private GameObject effectInstance;

        private PyroController pc;
        private ChildLocator childLocator;


        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;

        private static float turnSpeed = 180f;
        private AimAnimator aimAnimator;
        private AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;
        private Vector3 currentAimVector;

        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.isSprinting = false;

            pc = GetComponent<PyroController>();
            pc.inNapalm = true;

            hasBegunFlamethrower = false;

            entryDuration = baseEntryDuration / attackSpeedStat;
            fireInterval = baseFireInterval / attackSpeedStat;

            PlayCrossfade("Gesture, Override", "FireSecondary", 0.1f);
            StartAimMode();

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
            }

            // fake "agile", only while hovering.
            if (!characterBody.HasBuff(SS2Content.Buffs.bdPyroJet))
            {
                if (!characterBody.HasBuff(SS2Content.Buffs.BuffWatchMetronome)) // watchmetronome also adds fake agile. TODO i guess: make a util for fake agility
                    characterBody.isSprinting = false;
            }

            aimAnimator = GetAimAnimator();
            if (aimAnimator)
            {
                animatorDirectionOverrideRequest = aimAnimator.RequestDirectionOverride(new Func<Vector3>(GetAimDirection));
            }
            currentAimVector = inputBank.aimDirection;

            if (crosshairPrefab)
            {
                crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(characterBody, crosshairPrefab, CrosshairUtils.OverridePriority.Skill);
            }
        }
        private Vector3 GetAimDirection()
        {
            return currentAimVector;
        }
        public override void Update()
        {
            base.Update();

            currentAimVector = Vector3.RotateTowards(currentAimVector, inputBank.aimDirection, Mathf.Deg2Rad * turnSpeed * Time.deltaTime, 0);
            if (effectInstance)
            {
                effectInstance.transform.forward = currentAimVector;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                if (characterBody.isSprinting)
                {
                    if (!characterBody.HasBuff(SS2Content.Buffs.bdPyroJet) && !characterBody.HasBuff(SS2Content.Buffs.BuffWatchMetronome))
                    {
                        outer.SetNextStateToMain();
                    }
                    else
                    {
                        characterDirection.moveVector = inputBank.aimDirection; // if we are sprinting while firing, stay facing forward
                    }
                }
            }

            stopwatch += Time.fixedDeltaTime;
            characterBody.SetAimTimer(2f);

            if (stopwatch >= entryDuration || HasBuff(SS2.SS2Content.Buffs.bdPyroPressure))
            {
                if (!hasBegunFlamethrower)
                {
                    hasBegunFlamethrower = true;
                    Util.PlaySound(startFireSoundLoopString, gameObject);
                    effectInstance = GameObject.Instantiate(flameEffectPrefab, childLocator.FindChild(muzzleString));
                    effectInstance.transform.forward = inputBank.aimDirection;
                }
            }

            if (hasBegunFlamethrower)
            {
                heatStopwatch += Time.fixedDeltaTime;
                flamethrowerStopwatch -= Time.fixedDeltaTime;
                // we recalculate this each time to accomodate for attack speed changes during
                fireInterval = baseFireInterval / attackSpeedStat;
                if (flamethrowerStopwatch <= 0)
                {
                    flamethrowerStopwatch += fireInterval;
                    Fire(muzzleString);
                }
                // we count this separate for fuel burn rate consistency
                if (heatStopwatch > baseFireInterval && pc != null)
                {
                    if (isAuthority)
                    {
                        pc.AddHeat(-heatCostPerSecond * baseFireInterval);
                    }
                    heatStopwatch -= baseFireInterval;
                }
            }

            if (isAuthority)
            {
                if (!inputBank.skill2.down || pc.heat <= 0f)
            {
                    outer.SetNextStateToMain();
                    return;
                }
            }
            
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
            pc.inNapalm = false;

            Util.PlaySound(stopFireSoundLoopString, gameObject);
            Util.PlaySound(exitSoundString, gameObject);
            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(SS2Content.Buffs.bdPyroPressure.buffIndex, pressureDuration);
            }

            if (effectInstance != null)
            {
                Destroy(effectInstance);
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

        private float GetCurrentProjectileSpeed()
        {
            float t = stopwatch / durationForMaxSpeed;
            return Mathf.Lerp(minProjectileSpeed, maxProjectileSpeed, t);
        }
        private void Fire(string muzzleString)
        {
            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                aimRay.direction = currentAimVector;

                GameObject prefab = projectilePrefab;
                if (pc.isHighHeat)
                {
                    prefab = igniteProjectilePrefab;
                }

                float damage = damagePerSecond * fireInterval * damageStat;
                DamageColorIndex color = DamageColorIndex.Default;

                DamageTypeCombo damageType = new DamageTypeCombo();
                damageType.damageType = DamageType.Generic;
                R2API.DamageAPI.AddModdedDamageType(ref damageType, SS2.Survivors.Pyro.PyroIgniteOnHit);
                damageType.damageSource = DamageSource.Secondary;

                ProjectileManager.instance.FireProjectile(
                    prefab,
                    aimRay.origin,
                    Util.QuaternionSafeLookRotation(aimRay.direction),
                    gameObject,
                    damage,
                    force,
                    RollCrit(),
                    color,
                    null,
                    speedOverride: GetCurrentProjectileSpeed(),
                    damageType
                    );
            }

            characterBody.AddSpreadBloom(spreadBloomPerSecond * fireInterval);
            AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);
        }

        // visualizes a projectiles path like AimThrowableBase, but uses simple aim direction instead of trajectory calculation
        // not useful here but could be somewhere else (chirr throw, knight banner slam)
        #region Arc Visualizer
        //public static GameObject arcVisualizerPrefab;
        //private LineRenderer arcVisualizerLineRenderer;
        //private CalculateArcPointsJob calculateArcPointsJob;
        //private JobHandle calculateArcPointsJobHandle;
        //private Action completeArcPointsVisualizerJobMethod;
        //private Vector3[] pointsBuffer = Array.Empty<Vector3>();
        //protected TrajectoryInfo currentTrajectoryInfo;
        //protected float totalGravity
        //{
        //    get
        //    {
        //        return Physics.gravity.y + Physics.gravity.y * extraGravity;
        //    }
        //}
        //private float extraGravity;
        //private float projectileLifetime;
        //private void SetupVisualizer()
        //{
        //    if (projectilePrefab.TryGetComponent(out AntiGravityForce antiGravityForce))
        //    {
        //        extraGravity = antiGravityForce.antiGravityCoefficient * -1f;
        //    }
        //    if (projectilePrefab.TryGetComponent(out ProjectileSimple projectileSimple))
        //    {
        //        projectileLifetime = projectileSimple.lifetime;
        //    }
        //    if (arcVisualizerPrefab)
        //    {
        //        arcVisualizerLineRenderer = GameObject.Instantiate<GameObject>(arcVisualizerPrefab, transform.position, Quaternion.identity).GetComponent<LineRenderer>();
        //        calculateArcPointsJob = default(CalculateArcPointsJob);
        //        completeArcPointsVisualizerJobMethod = new Action(CompleteArcVisualizerJob);
        //        RoR2Application.onLateUpdate += completeArcPointsVisualizerJobMethod;
        //    }
        //}
        //private void CleanupVisualizer()
        //{
        //    calculateArcPointsJobHandle.Complete();
        //    if (arcVisualizerLineRenderer)
        //    {
        //        Destroy(arcVisualizerLineRenderer.gameObject);
        //        arcVisualizerLineRenderer = null;
        //    }
        //    if (completeArcPointsVisualizerJobMethod != null)
        //    {
        //        RoR2Application.onLateUpdate -= completeArcPointsVisualizerJobMethod;
        //        completeArcPointsVisualizerJobMethod = null;
        //    }
        //}
        //public override void Update()
        //{
        //    base.Update();
        //    UpdateTrajectoryInfo(out currentTrajectoryInfo);
        //    UpdateVisualizers(currentTrajectoryInfo);
        //}
        //private void UpdateTrajectoryInfo(out TrajectoryInfo dest)
        //{
        //    dest = default(TrajectoryInfo);
        //    Ray aimRay = GetAimRay();

        //    dest.speedOverride = GetCurrentProjectileSpeed();
        //    dest.finalRay = aimRay;
        //    dest.travelTime = projectileLifetime;
        //}
        //private void UpdateVisualizers(TrajectoryInfo trajectoryInfo)
        //{
        //    if (arcVisualizerLineRenderer && calculateArcPointsJobHandle.IsCompleted)
        //    {
        //        calculateArcPointsJob.SetParameters(trajectoryInfo.finalRay.origin, trajectoryInfo.finalRay.direction * trajectoryInfo.speedOverride, trajectoryInfo.travelTime, arcVisualizerLineRenderer.positionCount, totalGravity);
        //        calculateArcPointsJobHandle = calculateArcPointsJob.Schedule(calculateArcPointsJob.outputPositions.Length, 32);
        //    }
        //}
        //private void CompleteArcVisualizerJob()
        //{
        //    calculateArcPointsJobHandle.Complete();
        //    if (arcVisualizerLineRenderer)
        //    {
        //        Array.Resize<Vector3>(ref pointsBuffer, calculateArcPointsJob.outputPositions.Length);
        //        calculateArcPointsJob.outputPositions.CopyTo(pointsBuffer);
        //        arcVisualizerLineRenderer.SetPositions(pointsBuffer);
        //    }
        //}

        //private struct CalculateArcPointsJob : IJobParallelFor, IDisposable
        //{
        //    public void SetParameters(Vector3 origin, Vector3 velocity, float totalTravelTime, int positionCount, float gravity)
        //    {
        //        this.origin = origin;
        //        this.velocity = velocity;
        //        if (this.outputPositions.Length != positionCount)
        //        {
        //            if (this.outputPositions.IsCreated)
        //            {
        //                this.outputPositions.Dispose();
        //            }
        //            this.outputPositions = new NativeArray<Vector3>(positionCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        //        }
        //        this.indexMultiplier = totalTravelTime / (float)(positionCount - 1);
        //        this.gravity = gravity;
        //    }

        //    public void Dispose()
        //    {
        //        if (this.outputPositions.IsCreated)
        //        {
        //            this.outputPositions.Dispose();
        //        }
        //    }

        //    public void Execute(int index)
        //    {
        //        float t = (float)index * this.indexMultiplier;
        //        this.outputPositions[index] = Trajectory.CalculatePositionAtTime(this.origin, this.velocity, t, this.gravity);
        //    }

        //    [ReadOnly]
        //    private Vector3 origin;

        //    [ReadOnly]
        //    private Vector3 velocity;

        //    [ReadOnly]
        //    private float indexMultiplier;

        //    [ReadOnly]
        //    private float gravity;

        //    [WriteOnly]
        //    public NativeArray<Vector3> outputPositions;
        //}
        //protected struct TrajectoryInfo
        //{
        //    public Ray finalRay;

        //    public float travelTime;

        //    public float speedOverride;
        //}
        #endregion
    }
}
