using RoR2;
using SS2;
using SS2.Components;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Ball form primary (airborne). Slams downward similar to Loader's GroundSlam / 
    /// Overwatch Wrecking Ball's slam. Accelerates downward, detonates a BlastAttack
    /// on ground contact with speed-scaled damage.
    /// </summary>
    public class BallSlam : BaseCharacterMain
    {
        public static float airControl = 0.15f;
        public static float minimumDuration = 0.15f;
        public static float verticalAcceleration = -80f;
        public static float exitSlowdownCoefficient = 0.3f;
        public static float exitVerticalVelocity = 8f;
        public static float blastRadius = 12f;
        public static float blastDamageCoefficient = 5f;
        public static float blastProcCoefficient = 1f;
        public static float blastForce = 2000f;
        public static float maxDuration = 10f;
        public static Vector3 blastBonusForce = Vector3.zero;

        public static string enterSoundString = "";
        public static GameObject blastEffectPrefab;
        public static GameObject blastImpactEffectPrefab;
        public static GameObject slamEffectPrefab;

        private NemToolbotController controller;
        private float previousAirControl;
        private bool detonateNextFrame;
        private GameObject slamEffectInstance;
        private float impactSpeed;
        private bool completedSlam;
        private bool subscribedMovementHit;
        private bool initialized;
        private bool addedFallDamageProtection;

        public override void OnEnter()
        {
            base.OnEnter();

            if (!gameObject.TryGetComponent(out controller) || !controller.SetBallForm(true))
            {
                SS2Log.Error("NemToolbot BallSlam: Missing or unconfigured NemToolbotController.");
                if (isAuthority)
                    outer.SetNextStateToMain();
                return;
            }

            // Landing damage is checked on the server, not just the motor's owner.
            if ((characterBody.bodyFlags & CharacterBody.BodyFlags.IgnoreFallDamage) == 0)
            {
                characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                addedFallDamageProtection = true;
            }

            // PlayCrossfade("Body", "BallSlam", 0.1f);

            if (isAuthority && characterMotor != null)
            {
                characterMotor.onMovementHit += OnMovementHit;
                subscribedMovementHit = true;
                impactSpeed = characterMotor.velocity.magnitude;
            }

            // Util.PlaySound(enterSoundString, gameObject);

            if (characterMotor != null)
            {
                previousAirControl = characterMotor.airControl;
                characterMotor.airControl = airControl;
            }
            initialized = true;

            if (slamEffectPrefab != null)
            {
                Transform modelChild = FindModelChild("Root");
                if (modelChild != null)
                {
                    slamEffectInstance = Object.Instantiate(slamEffectPrefab, modelChild);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!initialized)
                return;

            if (isAuthority && characterMotor != null)
            {
                if (fixedAge >= minimumDuration && (detonateNextFrame || characterMotor.Motor.GroundingStatus.IsStableOnGround))
                {
                    completedSlam = true;
                    DetonateAuthority();
                    outer.SetNextStateToMain();
                    return;
                }

                // Allow limited steering during descent
                characterMotor.moveDirection = inputBank.moveVector;
                if (characterDirection != null)
                {
                    characterDirection.moveVector = characterMotor.moveDirection;
                }

                // Accelerate downward faster than normal gravity
                characterMotor.velocity.y += verticalAcceleration * GetDeltaTime();
                if (!detonateNextFrame)
                    impactSpeed = characterMotor.velocity.magnitude;

                // Safety timeout if we never hit the ground
                if (fixedAge >= maxDuration)
                {
                    completedSlam = true;
                    DetonateAuthority();
                    outer.SetNextStateToMain();
                    return;
                }
            }
        }

        public override void OnExit()
        {
            if (subscribedMovementHit && characterMotor)
                characterMotor.onMovementHit -= OnMovementHit;

            if (addedFallDamageProtection && characterBody)
                characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            if (isAuthority && completedSlam && characterMotor && characterBody.healthComponent.alive)
            {
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity *= exitSlowdownCoefficient;
                characterMotor.velocity.y = exitVerticalVelocity;
            }

            if (initialized && characterMotor != null)
            {
                characterMotor.airControl = previousAirControl;
            }

            if (slamEffectInstance != null)
            {
                EntityState.Destroy(slamEffectInstance);
            }

            base.OnExit();
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            if (!detonateNextFrame)
                impactSpeed = movementHitInfo.velocity.magnitude;
            detonateNextFrame = true;
        }

        private BlastAttack.Result DetonateAuthority()
        {
            if (!isAuthority)
                return default;

            Vector3 footPosition = characterBody.footPosition;

            float speedMultiplier = controller.GetDamageMultiplierFromSpeed(impactSpeed);

            if (blastEffectPrefab != null)
            {
                EffectManager.SpawnEffect(blastEffectPrefab, new EffectData
                {
                    origin = footPosition,
                    scale = blastRadius
                }, transmit: true);
            }

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.attacker = gameObject;
            blastAttack.baseDamage = damageStat * blastDamageCoefficient * speedMultiplier;
            blastAttack.baseForce = blastForce;
            blastAttack.bonusForce = blastBonusForce;
            blastAttack.crit = RollCrit();
            blastAttack.damageType = DamageType.Stun1s;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.procCoefficient = blastProcCoefficient;
            blastAttack.radius = blastRadius;
            blastAttack.position = footPosition;
            blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            if (blastImpactEffectPrefab != null)
            {
                blastAttack.impactEffect = EffectCatalog.FindEffectIndexFromPrefab(blastImpactEffectPrefab);
            }
            blastAttack.teamIndex = teamComponent.teamIndex;
            blastAttack.damageType.damageSource = DamageSource.Primary;

            return blastAttack.Fire();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
