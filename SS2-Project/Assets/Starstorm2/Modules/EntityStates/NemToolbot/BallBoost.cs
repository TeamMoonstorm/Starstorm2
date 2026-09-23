using System;
using System.Collections.Generic;
using RoR2;
using SS2;
using SS2.Components;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Ball form primary (grounded). Boosts forward at high speed with an OverlapAttack,
    /// dealing speed-scaled damage to enemies in the path. Similar to Mul-T's ToolbotDash.
    /// If used while airborne, immediately dispatches to BallSlam instead.
    /// </summary>
    public class BallBoost : BaseSkillState
    {
        [SerializeField]
        public float baseDuration = 1.5f;

        [SerializeField]
        public float speedMultiplier = 2.5f;

        public static float baseDamageCoefficient = 4f;
        public static float awayForceMagnitude = 1000f;
        public static float upwardForceMagnitude = 500f;
        public static float hitPauseDuration = 0.1f;
        public static float recoilAmplitude = 1f;

        public static string startSoundString = "";
        public static string endSoundString = "";
        public static string impactSoundString = "";
        public static GameObject startEffectPrefab;
        public static GameObject impactEffectPrefab;

        private NemToolbotController controller;
        private float duration;
        private float hitPauseTimer;
        private Vector3 idealDirection;
        private OverlapAttack attack;
        private bool inHitPause;
        private bool startedBoost;
        private bool completedBoost;
        private List<HurtBox> victimsStruck = new List<HurtBox>();

        public override void OnEnter()
        {
            base.OnEnter();

            if (!gameObject.TryGetComponent(out controller) || !controller.SetBallForm(true))
            {
                SS2Log.Error("NemToolbot BallBoost: Missing or unconfigured NemToolbotController.");
                if (isAuthority)
                    outer.SetNextStateToMain();
                return;
            }

            // If airborne, dispatch to BallSlam instead
            if (isAuthority && characterMotor != null && !characterMotor.isGrounded)
            {
                outer.SetNextState(new BallSlam());
                return;
            }

            duration = baseDuration;

            if (isAuthority && inputBank != null)
            {
                idealDirection = inputBank.aimDirection;
                idealDirection.y = 0f;
                idealDirection.Normalize();
                if (idealDirection == Vector3.zero)
                    idealDirection = characterDirection.forward;
            }

            if (isAuthority && characterDirection != null)
            {
                characterDirection.forward = idealDirection;
            }

            if (startEffectPrefab != null && characterBody != null)
            {
                EffectManager.SpawnEffect(startEffectPrefab, new EffectData
                {
                    origin = characterBody.corePosition
                }, transmit: false);
            }

            // Util.PlaySound(startSoundString, gameObject);

            if (isAuthority && characterBody != null)
            {
                characterBody.isSprinting = true;
            }

            // Build overlap attack
            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = GetModelTransform();
            if (modelTransform != null)
            {
                hitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Charge");
            }
            if (!hitBoxGroup)
            {
                SS2Log.Error("NemToolbot BallBoost: Model is missing the Charge HitBoxGroup.");
                if (isAuthority)
                    outer.SetNextStateToMain();
                return;
            }

            attack = new OverlapAttack();
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = baseDamageCoefficient * damageStat;
            attack.hitEffectPrefab = impactEffectPrefab;
            attack.forceVector = Vector3.up * upwardForceMagnitude;
            attack.pushAwayForce = awayForceMagnitude;
            attack.hitBoxGroup = hitBoxGroup;
            attack.damageType.damageSource = DamageSource.Primary;
            attack.isCrit = RollCrit();
            startedBoost = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!startedBoost)
                return;

            if (fixedAge >= duration)
            {
                if (isAuthority)
                {
                    completedBoost = true;
                    outer.SetNextStateToMain();
                }
                return;
            }

            if (!isAuthority)
                return;

            if (!inHitPause)
            {
                UpdateDirection();

                if (characterDirection != null)
                {
                    characterDirection.moveVector = idealDirection;
                }

                Vector3 boostVelocity = Vector3.zero;
                if (characterMotor != null && !characterMotor.disableAirControlUntilCollision)
                {
                    boostVelocity = GetIdealVelocity();
                    characterMotor.rootMotion += boostVelocity * GetDeltaTime();
                }

                // Scale damage by speed
                float speedMultFromVelocity = controller.GetDamageMultiplierFromSpeed((characterMotor.velocity + boostVelocity).magnitude);
                attack.damage = damageStat * (baseDamageCoefficient * speedMultFromVelocity);

                if (attack.Fire(victimsStruck))
                {
                    // Util.PlaySound(impactSoundString, gameObject);
                    inHitPause = true;
                    hitPauseTimer = hitPauseDuration;
                    AddRecoil(-0.5f * recoilAmplitude, -0.5f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
                }
            }
            else
            {
                if (characterMotor != null)
                {
                    characterMotor.velocity = Vector3.zero;
                }
                hitPauseTimer -= GetDeltaTime();
                if (hitPauseTimer < 0f)
                {
                    inHitPause = false;
                }
            }
        }

        public override void OnExit()
        {
            // if (startedBoost)
            //     Util.PlaySound(endSoundString, gameObject);

            if (isAuthority && completedBoost && characterBody.healthComponent.alive &&
                characterMotor != null && !characterMotor.disableAirControlUntilCollision)
            {
                characterMotor.velocity += GetIdealVelocity();
            }

            base.OnExit();
        }

        private void UpdateDirection()
        {
            if (inputBank != null)
            {
                Vector2 moveInput = Util.Vector3XZToVector2XY(inputBank.moveVector);
                if (moveInput != Vector2.zero)
                {
                    moveInput.Normalize();
                    idealDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
                }
            }
        }

        private Vector3 GetIdealVelocity()
        {
            if (characterDirection == null || characterBody == null)
                return Vector3.zero;

            return characterDirection.forward * characterBody.moveSpeed * speedMultiplier;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
