using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2
{
    [RequireComponent(typeof(BeamController))]
    [DisallowMultipleComponent]
    public class LampBeamPullController : NetworkBehaviour
    {
        private const float forceMagnitude = 1.25f;
        private const float maxHeightDiff = 15f;
        private const float forceCoefficientAtMaxHeightDiff = 2f;
        private const float bonusPerBeam = 0.03f;
        private const int maxBonusStacks = 4;

        [SyncVar]
        private bool hasLiftPermission;
        [SyncVar]
        private int bonusStacks;

        private BeamController beam;
        private bool attemptedClaim;
        private LiftTargetState targetState;
        private Transform sourceMuzzle;

        private void Awake()
        {
            beam = GetComponent<BeamController>();
        }

        private void OnEnable()
        {
            beam.onBreakServer += ReleaseLiftServer;
        }

        private void OnDisable()
        {
            beam.onBreakServer -= ReleaseLiftServer;
            if (NetworkServer.active)
            {
                ReleaseLiftServer();
            }
        }

        [Server]
        public void TryClaimLiftServer()
        {
            if (attemptedClaim)
            {
                return;
            }
            attemptedClaim = true;
            if (!TryGetEligibleTarget(out _, out HealthComponent victim))
            {
                return;
            }

            targetState = GetTargetState(victim);
            LampBeamPullController previousOwner = targetState.owner;
            if (previousOwner)
            {
                if (previousOwner.TryGetEligibleTarget(out _, out HealthComponent previousVictim) && previousVictim == victim)
                {
                    return;
                }
                previousOwner.ReleaseLiftServer();
            }

            targetState.owner = this;
            hasLiftPermission = true;
            bonusStacks = CountBonusStacks(victim);
        }

        [Server]
        private void ReleaseLiftServer()
        {
            if (targetState && targetState.owner == this)
            {
                targetState.owner = null;
            }
            hasLiftPermission = false;
            bonusStacks = 0;
        }

        private bool TryGetLiveTarget(out CharacterBody source, out HealthComponent victim)
        {
            source = null;
            victim = null;
            if (!isActiveAndEnabled || !beam.isActiveAndEnabled || beam.isBroken)
            {
                return false;
            }
            GameObject sourceObject = beam.ownership.ownerObject;
            HurtBox target = beam.target;
            if (!sourceObject || !target || !target.healthComponent)
            {
                return false;
            }
            source = sourceObject.GetComponent<CharacterBody>();
            victim = target.healthComponent;
            return source && source.healthComponent && source.healthComponent.alive && victim.alive && victim.body;
        }

        private bool TryGetEligibleTarget(out CharacterBody source, out HealthComponent victim)
        {
            return TryGetLiveTarget(out source, out victim)
                && FriendlyFireManager.ShouldSeekingProceed(victim, source.teamComponent.teamIndex);
        }

        private int CountBonusStacks(HealthComponent victim)
        {
            int count = 0;
            foreach (BeamController candidate in InstanceTracker.GetInstancesList<BeamController>())
            {
                if (candidate && candidate != beam && candidate.TryGetComponent(out LampBeamPullController pull)
                    && pull.TryGetEligibleTarget(out _, out HealthComponent candidateVictim) && candidateVictim == victim)
                {
                    count++;
                    if (count == maxBonusStacks)
                    {
                        break;
                    }
                }
            }
            return count;
        }

        private void FixedUpdate()
        {
            if (!hasLiftPermission)
            {
                return;
            }
            if (!TryGetLiveTarget(out CharacterBody source, out HealthComponent victim)
                || (NetworkServer.active && !FriendlyFireManager.ShouldSeekingProceed(victim, source.teamComponent.teamIndex)))
            {
                if (NetworkServer.active)
                {
                    ReleaseLiftServer();
                }
                return;
            }
            if (NetworkServer.active)
            {
                bonusStacks = CountBonusStacks(victim);
            }

            if (!victim.body.hasEffectiveAuthority
                || (victim.body.HasBuff(RoR2Content.Buffs.EngiShield) && victim.shield > 0f))
            {
                return;
            }

            targetState = GetTargetState(victim);
            if (!targetState.TryApplyThisStep())
            {
                return;
            }

            float heightDifference = beam.target.transform.position.y - source.transform.position.y;
            PhysForceInfo forceInfo = PhysForceInfo.Create();
            forceInfo.force = CalculatePullForce(heightDifference, bonusStacks);
            forceInfo.massIsOne = true;
            forceInfo.ignoreGroundStick = true;
            forceInfo.doNotExceed = true;
            IPhysMotor motor = victim.GetComponent<IPhysMotor>();
            if (motor != null)
            {
                motor.ApplyForceImpulse(in forceInfo);
            }
            else if (victim.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForceWithInfo(forceInfo);
            }
        }

        internal static Vector3 CalculatePullForce(float heightDifference, int extraBeamCount)
        {
            Vector3 direction = new Vector3(0f, -heightDifference, 0f).normalized;
            float strength = forceMagnitude * Mathf.Clamp(Mathf.Abs(heightDifference) / maxHeightDiff, 1f, forceCoefficientAtMaxHeightDiff);
            if (direction.y > 0f)
            {
                strength *= 1f + bonusPerBeam * Mathf.Clamp(extraBeamCount, 0, maxBonusStacks);
            }
            return direction * strength;
        }

        private void Update()
        {
            GameObject sourceObject = beam.ownership.ownerObject;
            if (!sourceObject)
            {
                sourceMuzzle = null;
                transform.SetParent(null, true);
                return;
            }
            if (!sourceMuzzle)
            {
                CharacterBody source = sourceObject.GetComponent<CharacterBody>();
                if (source && source.modelLocator && source.modelLocator.modelTransform)
                {
                    ChildLocator childLocator = source.modelLocator.modelTransform.GetComponent<ChildLocator>();
                    if (childLocator)
                    {
                        sourceMuzzle = childLocator.FindChild("Muzzle");
                    }
                }
            }
            if (sourceMuzzle && transform.parent != sourceMuzzle)
            {
                // Network spawning does not replicate the server's muzzle parenting.
                transform.SetParent(sourceMuzzle, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }

        private static LiftTargetState GetTargetState(HealthComponent victim)
        {
            LiftTargetState state = victim.GetComponent<LiftTargetState>();
            return state ? state : victim.gameObject.AddComponent<LiftTargetState>();
        }

        public class LiftTargetState : MonoBehaviour
        {
            public LampBeamPullController owner;
            private float lastAppliedFixedTime = float.NegativeInfinity;

            public bool TryApplyThisStep()
            {
                // Keep this guard across owner changes, including overlapping network updates.
                if (lastAppliedFixedTime == Time.fixedTime)
                {
                    return false;
                }
                lastAppliedFixedTime = Time.fixedTime;
                return true;
            }

            private void OnDisable()
            {
                if (NetworkServer.active && owner)
                {
                    owner.ReleaseLiftServer();
                }
            }
        }
    }
}
