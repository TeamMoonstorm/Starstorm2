using RoR2;
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

        public override void OnEnter()
        {
            base.OnEnter();

            if (!gameObject.TryGetComponent(out controller))
            {
                Debug.LogError("NemToolbot BallSlam: Failed to get NemToolbotController on " + gameObject.name);
            }

            PlayCrossfade("Body", "BallSlam", 0.1f);

            if (isAuthority && characterMotor != null)
            {
                characterMotor.onMovementHit += OnMovementHit;
            }

            Util.PlaySound(enterSoundString, gameObject);

            if (characterMotor != null)
            {
                previousAirControl = characterMotor.airControl;
                characterMotor.airControl = airControl;
            }

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

            if (isAuthority && characterMotor != null)
            {
                // Allow limited steering during descent
                characterMotor.moveDirection = inputBank.moveVector;
                if (characterDirection != null)
                {
                    characterDirection.moveVector = characterMotor.moveDirection;
                }

                // Accelerate downward faster than normal gravity
                characterMotor.velocity.y += verticalAcceleration * GetDeltaTime();

                // Safety timeout if we never hit the ground
                if (fixedAge >= maxDuration)
                {
                    DetonateAuthority();
                    outer.SetNextStateToMain();
                    return;
                }

                // Detonate when hitting the ground after minimum duration
                if (fixedAge >= minimumDuration && (detonateNextFrame || characterMotor.Motor.GroundingStatus.IsStableOnGround))
                {
                    DetonateAuthority();
                    outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            if (isAuthority && characterMotor != null)
            {
                characterMotor.onMovementHit -= OnMovementHit;
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity *= exitSlowdownCoefficient;
                characterMotor.velocity.y = exitVerticalVelocity;
            }

            if (characterMotor != null)
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
            detonateNextFrame = true;
        }

        private BlastAttack.Result DetonateAuthority()
        {
            if (!isAuthority)
                return default;

            Vector3 footPosition = characterBody.footPosition;

            float speedMultiplier = controller != null ? controller.GetDamageMultiplierFromSpeed() : 1f;

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
            blastAttack.damageType.damageSource = DamageSource.Special;

            return blastAttack.Fire();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
