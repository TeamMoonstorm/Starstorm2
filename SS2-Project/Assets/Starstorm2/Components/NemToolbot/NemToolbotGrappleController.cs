using System;
using EntityStates;
using EntityStates.NemToolbot;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Projectile;

namespace SS2.Components
{
    /// <summary>
    /// Projectile component that drives NemToolbot's grappling hook lifecycle.
    /// Based on Loader's ProjectileGrappleController with identical feel.
    ///
    /// State pipeline:
    ///   FlyState --> GripState (heavy target or world) --> ReturnState
    ///           |-> YankState (light enemy)            --> ReturnState
    ///           |-> ReturnState (missed / max distance)
    ///
    /// Changes from Loader's version:
    /// - Casts to FireGrapplingHook instead of FireHook
    /// - Configurable ESM customName (hookStateMachineName)
    /// - TryGetComponent everywhere with Debug.LogError on failure
    /// - Configurable stock deduction slot
    /// </summary>
    [RequireComponent(typeof(EntityStateMachine))]
    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(ProjectileSimple))]
    [RequireComponent(typeof(ProjectileStickOnImpact))]
    public class NemToolbotGrappleController : MonoBehaviour
    {
        private struct OwnerInfo
        {
            public readonly GameObject gameObject;
            public readonly CharacterBody characterBody;
            public readonly CharacterMotor characterMotor;
            public readonly EntityStateMachine stateMachine;
            public readonly bool hasEffectiveAuthority;

            public OwnerInfo(GameObject ownerGameObject, string hookESMName)
            {
                this = default(OwnerInfo);
                gameObject = ownerGameObject;
                if (!gameObject)
                {
                    return;
                }

                if (!gameObject.TryGetComponent(out CharacterBody body))
                {
                    Debug.LogError("NemToolbotGrappleController: Owner missing CharacterBody on " + gameObject.name);
                }
                characterBody = body;

                if (!gameObject.TryGetComponent(out CharacterMotor motor))
                {
                    Debug.LogError("NemToolbotGrappleController: Owner missing CharacterMotor on " + gameObject.name);
                }
                characterMotor = motor;

                hasEffectiveAuthority= Util.HasEffectiveAuthority(gameObject);

                EntityStateMachine[] esms = gameObject.GetComponents<EntityStateMachine>();
                for (int i = 0; i < esms.Length; i++)
                {
                    if (esms[i].customName == hookESMName)
                    {
                        stateMachine = esms[i];
                        break;
                    }
                }

                if (!stateMachine)
                {
                    Debug.LogError("NemToolbotGrappleController: Owner missing ESM with customName '" + hookESMName + "' on " + gameObject.name);
                }
            }
        }

        #region Inner States

        /// <summary>
        /// Shared foundation for all grapple hook states. Validates owner each tick,
        /// updates spatial data, and auto-retracts when owner exits firing state.
        /// </summary>
        private class BaseState : EntityStates.BaseState
        {
            protected NemToolbotGrappleController grappleController;
            protected Vector3 aimOrigin;
            protected Vector3 position;

            protected bool ownerValid { get; private set; }
            protected ref OwnerInfo owner => ref grappleController.owner;

            private void UpdatePositions()
            {
                aimOrigin = grappleController.owner.characterBody.aimOrigin;
                position = base.transform.position + base.transform.up * grappleController.normalOffset;
            }

            public override void OnEnter()
            {
                base.OnEnter();
                if (!base.gameObject.TryGetComponent(out NemToolbotGrappleController controller))
                {
                    Debug.LogError("NemToolbotGrappleController.BaseState: Missing NemToolbotGrappleController on " + base.gameObject.name);
                    return;
                }
                grappleController = controller;
                ownerValid = (bool)grappleController && (bool)grappleController.owner.gameObject;
                if (ownerValid)
                {
                    UpdatePositions();
                }
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (ownerValid)
                {
                    ownerValid &= grappleController.owner.gameObject;
                    if (ownerValid)
                    {
                        UpdatePositions();
                        FixedUpdateBehavior();
                    }
                }
                if (NetworkServer.active && !ownerValid)
                {
                    ownerValid = false;
                    EntityState.Destroy(base.gameObject);
                }
            }

            protected virtual void FixedUpdateBehavior()
            {
                if (base.isAuthority && !grappleController.OwnerIsInFiringState())
                {
                    outer.SetNextState(new ReturnState());
                }
            }

            protected Ray GetOwnerAimRay()
            {
                if (!owner.characterBody)
                {
                    return default(Ray);
                }
                return owner.characterBody.inputBank.GetAimRay();
            }
        }

        /// <summary>
        /// Hook is traveling outward. On impact, transitions to GripState (heavy/world)
        /// or YankState (light enemy). Auto-retracts if max distance reached.
        /// </summary>
        private class FlyState : BaseState
        {
            private float duration;

            public override void OnEnter()
            {
                base.OnEnter();
                if (grappleController != null)
                {
                    duration = grappleController.maxTravelDistance / grappleController.projectileSimple.velocity;
                }
            }

            protected override void FixedUpdateBehavior()
            {
                base.FixedUpdateBehavior();
                if (!base.isAuthority)
                {
                    return;
                }

                if (grappleController.projectileStickOnImpactController.stuck)
                {
                    EntityState nextState = null;

                    if ((bool)grappleController.projectileStickOnImpactController.stuckBody)
                    {
                        if (grappleController.projectileStickOnImpactController.stuckBody.TryGetComponent(out Rigidbody stuckRb))
                        {
                            if (stuckRb.mass < grappleController.yankMassLimit)
                            {
                                stuckRb.TryGetComponent(out CharacterBody stuckCharBody);
                                if (!stuckCharBody
                                    || !stuckCharBody.isPlayerControlled
                                    || stuckCharBody.teamComponent.teamIndex != base.projectileController.teamFilter.teamIndex
                                    || FriendlyFireManager.ShouldDirectHitProceed(stuckCharBody.healthComponent, base.projectileController.teamFilter.teamIndex))
                                {
                                    nextState = new YankState();
                                }
                            }
                        }
                    }

                    if (nextState == null)
                    {
                        nextState = new GripState();
                    }

                    DeductOwnerStock();
                    outer.SetNextState(nextState);
                }
                else if (duration <= base.fixedAge)
                {
                    outer.SetNextState(new ReturnState());
                }
            }

            private void DeductOwnerStock()
            {
                if (!base.ownerValid || !base.owner.hasEffectiveAuthority)
                {
                    return;
                }

                if (base.owner.gameObject.TryGetComponent(out SkillLocator skillLocator))
                {
                    GenericSkill skill = grappleController.GetDeductionSkill(skillLocator);
                    if ((bool)skill)
                    {
                        skill.DeductStock(1);
                    }
                }
                else
                {
                    Debug.LogError("NemToolbotGrappleController.FlyState: Owner missing SkillLocator on " + base.owner.gameObject.name);
                }
            }
        }

        /// <summary>
        /// Shared base for GripState and YankState. Tracks distance and handles
        /// release conditions: unstuck, near-break, button release, state exit.
        /// </summary>
        private class BaseGripState : BaseState
        {
            protected float currentDistance;

            public override void OnEnter()
            {
                base.OnEnter();
                currentDistance = Vector3.Distance(aimOrigin, position);
            }

            protected override void FixedUpdateBehavior()
            {
                base.FixedUpdateBehavior();
                currentDistance = Vector3.Distance(aimOrigin, position);
                if (base.isAuthority)
                {
                    bool hookDetached = !grappleController.projectileStickOnImpactController.stuck;
                    bool arrivedAtHook = currentDistance < grappleController.nearBreakDistance;
                    bool ownerLeftFiringState = !grappleController.OwnerIsInFiringState();

                    if (!base.owner.stateMachine
                        || !((base.owner.stateMachine.state as BaseSkillState)?.IsKeyDownAuthority() ?? false)
                        || ownerLeftFiringState
                        || arrivedAtHook
                        || hookDetached)
                    {
                        outer.SetNextState(new ReturnState());
                    }
                }
            }
        }

        /// <summary>
        /// Pulls the PLAYER toward the hook point (grapple swing). Identical force model
        /// to Loader's GripState: base acceleration + look steering + move input + escape force.
        /// </summary>
        private class GripState : BaseGripState
        {
            private float lastDistance;

            public override void OnEnter()
            {
                base.OnEnter();
                lastDistance = Vector3.Distance(aimOrigin, position);
                if (base.ownerValid)
                {
                    if ((bool)base.owner.characterMotor)
                    {
                        Vector3 direction = GetOwnerAimRay().direction;
                        Vector3 velocity = base.owner.characterMotor.velocity;
                        velocity = ((Vector3.Dot(velocity, direction) < 0f) ? Vector3.zero : Vector3.Project(velocity, direction));
                        velocity += direction * grappleController.initialLookImpulse;
                        velocity += base.owner.characterMotor.moveDirection * grappleController.initiallMoveImpulse;
                        base.owner.characterMotor.velocity = velocity;
                    }
                }
            }

            protected override void FixedUpdateBehavior()
            {
                base.FixedUpdateBehavior();
                float accel = grappleController.acceleration;

                if (currentDistance > lastDistance)
                {
                    accel *= grappleController.escapeForceMultiplier;
                }
                lastDistance = currentDistance;

                if (base.owner.hasEffectiveAuthority && (bool)base.owner.characterMotor && (bool)base.owner.characterBody)
                {
                    Ray ownerAimRay = GetOwnerAimRay();
                    Vector3 hookDirection = (base.transform.position - base.owner.characterBody.aimOrigin).normalized;
                    Vector3 force = hookDirection * accel;

                    float time = Mathf.Clamp01(base.fixedAge / grappleController.lookAccelerationRampUpDuration);
                    float rampUpFactor = grappleController.lookAccelerationRampUpCurve.Evaluate(time);
                    float alignmentFactor = Util.Remap(Vector3.Dot(ownerAimRay.direction, hookDirection), -1f, 1f, 1f, 0f);
                    force += ownerAimRay.direction * (grappleController.lookAcceleration * rampUpFactor * alignmentFactor);

                    force += base.owner.characterMotor.moveDirection * grappleController.moveAcceleration;

                    base.owner.characterMotor.ApplyForce(force * (base.owner.characterMotor.mass * GetDeltaTime()), alwaysApply: true, disableAirControlUntilCollision: true);
                }
            }
        }

        /// <summary>
        /// Pulls a LIGHT ENEMY toward the player. Identical to Loader's YankState:
        /// IDisplacementReceiver pull with delay, brief owner hover.
        /// </summary>
        private class YankState : BaseGripState
        {
            public static float yankSpeed;
            public static float delayBeforeYanking;
            public static float hoverTimeLimit = 0.5f;

            private CharacterBody stuckBody;

            public override void OnEnter()
            {
                base.OnEnter();
                stuckBody = grappleController.projectileStickOnImpactController.stuckBody;
            }

            protected override void FixedUpdateBehavior()
            {
                base.FixedUpdateBehavior();
                if (!stuckBody)
                {
                    return;
                }

                if (Util.HasEffectiveAuthority(stuckBody.gameObject))
                {
                    Vector3 pullDirection = aimOrigin - position;
                    if (stuckBody.TryGetComponent(out IDisplacementReceiver displacementReceiver) && base.fixedAge >= delayBeforeYanking)
                    {
                        displacementReceiver.AddDisplacement(pullDirection * (yankSpeed * GetDeltaTime()));
                    }
                }

                if (base.owner.hasEffectiveAuthority && (bool)base.owner.characterMotor && base.fixedAge < hoverTimeLimit)
                {
                    Vector3 velocity = base.owner.characterMotor.velocity;
                    if (velocity.y < 0f)
                    {
                        velocity.y = 0f;
                        base.owner.characterMotor.velocity = velocity;
                    }
                }
            }
        }

        /// <summary>
        /// Hook retracts back to the player. Detaches, disables collider,
        /// accelerates toward aimOrigin, and self-destructs on overshoot.
        /// </summary>
        private class ReturnState : BaseState
        {
            private float returnSpeedAcceleration = 240f;
            private float returnSpeed;

            public override void OnEnter()
            {
                base.OnEnter();
                if (base.ownerValid)
                {
                    returnSpeed = grappleController.projectileSimple.velocity;
                    returnSpeedAcceleration = returnSpeed * 2f;
                }

                if (NetworkServer.active && (bool)grappleController)
                {
                    grappleController.projectileStickOnImpactController.Detach();
                    grappleController.projectileStickOnImpactController.ignoreCharacters = true;
                    grappleController.projectileStickOnImpactController.ignoreWorld = true;
                }

                if (base.gameObject.TryGetComponent(out Collider collider))
                {
                    collider.enabled = false;
                }
            }

            protected override void FixedUpdateBehavior()
            {
                base.FixedUpdateBehavior();
                if (!base.rigidbody)
                {
                    return;
                }

                returnSpeed += returnSpeedAcceleration * GetDeltaTime();
                base.rigidbody.velocity = (aimOrigin - position).normalized * returnSpeed;

                if (NetworkServer.active)
                {
                    Vector3 endPosition = position + base.rigidbody.velocity * GetDeltaTime();
                    if (HGMath.Overshoots(position, endPosition, aimOrigin))
                    {
                        EntityState.Destroy(base.gameObject);
                    }
                }
            }
        }

        #endregion

        #region Controller Fields

        private ProjectileController projectileController;
        private ProjectileStickOnImpact projectileStickOnImpactController;
        private ProjectileSimple projectileSimple;

        /// <summary>
        /// The EntityState type the owner must be in for the hook to remain active.
        /// Set to FireGrapplingHook in the inspector. If the owner exits this state, the hook retracts.
        /// </summary>
        public SerializableEntityStateType ownerHookStateType;

        /// <summary>
        /// Name of the EntityStateMachine on the owner that runs the hook firing state.
        /// Default "Hook". Must match the ESM customName on the NemToolbot character body.
        /// </summary>
        [Tooltip("ESM customName on the owner body that runs the hook firing state.")]
        public string hookStateMachineName = "Hook";

        /// <summary>
        /// Which skill slot to deduct stock from on successful hook impact.
        /// </summary>
        public enum DeductSlot
        {
            Primary,
            Secondary,
            Utility,
            Special,
            None
        }

        [Tooltip("Which skill slot to deduct stock from when the hook sticks.")]
        public DeductSlot deductStockSlot = DeductSlot.Secondary;

        public float acceleration;
        public float lookAcceleration = 4f;
        public float lookAccelerationRampUpDuration = 0.25f;
        public float initialLookImpulse = 5f;
        public float initiallMoveImpulse = 5f;
        public float moveAcceleration = 4f;

        public string enterSoundString;
        public string exitSoundString;
        public string hookDistanceRTPCstring;
        public float minHookDistancePitchModifier;
        public float maxHookDistancePitchModifier;

        public AnimationCurve lookAccelerationRampUpCurve;
        public Transform ropeEndTransform;
        public string muzzleStringOnBody = "MuzzleLeft";

        [Tooltip("Minimum distance before the hook auto-detaches (arrival).")]
        public float nearBreakDistance;

        [Tooltip("Maximum travel distance for the hook.")]
        public float maxTravelDistance;

        public float escapeForceMultiplier = 2f;
        public float normalOffset = 1f;
        public float yankMassLimit;

        private Type resolvedOwnerHookStateType;
        private OwnerInfo owner;
        private uint soundID;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (!TryGetComponent(out projectileStickOnImpactController))
            {
                Debug.LogError("NemToolbotGrappleController: Missing ProjectileStickOnImpact on " + gameObject.name);
            }
            if (!TryGetComponent(out projectileController))
            {
                Debug.LogError("NemToolbotGrappleController: Missing ProjectileController on " + gameObject.name);
            }
            if (!TryGetComponent(out projectileSimple))
            {
                Debug.LogError("NemToolbotGrappleController: Missing ProjectileSimple on " + gameObject.name);
            }

            resolvedOwnerHookStateType = ownerHookStateType.stateType;

            if ((bool)ropeEndTransform)
            {
                soundID = Util.PlaySound(enterSoundString, ropeEndTransform.gameObject);
            }
        }

        private void Start()
        {
            if (projectileController != null && projectileController.owner != null)
            {
                owner = new OwnerInfo(projectileController.owner, hookStateMachineName);
                AssignHookReferenceToBodyStateMachine();
            }
            else
            {
                Debug.LogError("NemToolbotGrappleController: projectileController or owner is null on Start for " + gameObject.name);
            }
        }

        private void FixedUpdate()
        {
            if ((bool)ropeEndTransform)
            {
                float in_value = Util.Remap(
                    (ropeEndTransform.transform.position - base.transform.position).magnitude,
                    minHookDistancePitchModifier,
                    maxHookDistancePitchModifier,
                    0f, 100f);
                AkSoundEngine.SetRTPCValueByPlayingID(hookDistanceRTPCstring, in_value, soundID);
            }
        }

        private void OnDestroy()
        {
            if ((bool)ropeEndTransform)
            {
                Util.PlaySound(exitSoundString, ropeEndTransform.gameObject);
                UnityEngine.Object.Destroy(ropeEndTransform.gameObject);
            }
            else
            {
                AkSoundEngine.StopPlayingID(soundID);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Links this hook projectile back to the owner's FireGrapplingHook state
        /// and parents the rope end to the owner's muzzle bone.
        /// </summary>
        private void AssignHookReferenceToBodyStateMachine()
        {
            if ((bool)owner.stateMachine && owner.stateMachine.state is FireGrapplingHook fireGrapplingHook)
            {
                fireGrapplingHook.SetHookReference(base.gameObject);
            }

            if (!owner.gameObject.TryGetComponent(out ModelLocator modelLocator))
            {
                Debug.LogError("NemToolbotGrappleController: Owner missing ModelLocator on " + owner.gameObject.name);
                return;
            }

            Transform modelTransform = modelLocator.modelTransform;
            if (!modelTransform)
            {
                return;
            }

            if (modelTransform.TryGetComponent(out ChildLocator childLocator))
            {
                Transform muzzle = childLocator.FindChild(muzzleStringOnBody);
                if ((bool)muzzle && (bool)ropeEndTransform)
                {
                    ropeEndTransform.SetParent(muzzle, worldPositionStays: false);
                }
            }
            else
            {
                Debug.LogError("NemToolbotGrappleController: Owner model missing ChildLocator on " + modelTransform.name);
            }
        }

        private bool OwnerIsInFiringState()
        {
            if ((bool)owner.stateMachine)
            {
                return owner.stateMachine.state.GetType() == resolvedOwnerHookStateType;
            }
            return false;
        }

        private GenericSkill GetDeductionSkill(SkillLocator skillLocator)
        {
            switch (deductStockSlot)
            {
                case DeductSlot.Primary:
                    return skillLocator.primary;
                case DeductSlot.Secondary:
                    return skillLocator.secondary;
                case DeductSlot.Utility:
                    return skillLocator.utility;
                case DeductSlot.Special:
                    return skillLocator.special;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Configures a projectile's EntityStateMachine to use this controller's inner FlyState.
        /// Call this when setting up the grapple hook projectile prefab in code.
        /// </summary>
        public static void ConfigureProjectileESM(EntityStateMachine esm)
        {
            esm.initialStateType = new SerializableEntityStateType(typeof(FlyState));
            esm.mainStateType = new SerializableEntityStateType(typeof(FlyState));
        }

        #endregion
    }
}
