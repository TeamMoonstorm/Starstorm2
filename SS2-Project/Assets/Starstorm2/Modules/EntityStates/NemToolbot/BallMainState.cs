using RoR2;
using SS2.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Body main state for ball form. Replaces GenericCharacterMain while NemToolbot
    /// is rolled up. Provides momentum-based movement with higher top speed, modified
    /// air control, and velocity tracking for speed-based damage scaling.
    /// </summary>
    public class BallMainState : BaseCharacterMain
    {
        public static float moveSpeedMultiplier = 1.6f;
        public static float ballAirControl = 0.15f;
        public static float accelerationMultiplier = 0.6f;
        public static float momentumDamping = 0.98f;

        public static string enterSoundString = "";
        public static string loopSoundString = "";
        public static string exitSoundString = "";

        private NemToolbotController controller;
        private float originalMoveSpeed;
        private float originalAcceleration;
        private float originalAirControl;
        private uint loopSoundID;

        private static readonly int ballModeHash = Animator.StringToHash("BallMode");
        private static readonly int aimWeightHash = Animator.StringToHash("aimWeight");

        public override void OnEnter()
        {
            base.OnEnter();

            if (!gameObject.TryGetComponent(out controller))
            {
                Debug.LogError("NemToolbot BallMainState: Failed to get NemToolbotController on " + gameObject.name);
            }

            // Store original motor parameters for restoration on exit
            if (characterBody != null)
            {
                originalMoveSpeed = characterBody.baseMoveSpeed;
                originalAcceleration = characterBody.baseAcceleration;
                characterBody.baseMoveSpeed *= moveSpeedMultiplier;
                characterBody.baseAcceleration *= accelerationMultiplier;
                characterBody.statsDirty = true;
            }

            if (characterMotor != null)
            {
                originalAirControl = characterMotor.airControl;
                characterMotor.airControl = ballAirControl;
            }

            if (NetworkServer.active && characterBody != null)
            {
                characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
            }

            Util.PlaySound(enterSoundString, gameObject);
            loopSoundID = Util.PlaySound(loopSoundString, gameObject);

            PlayCrossfade("Body", "BallModeEnter", 0.1f);
            if (modelAnimator != null)
            {
                modelAnimator.SetFloat(aimWeightHash, 0f);
            }

            if (modelLocator != null)
            {
                modelLocator.normalizeToFloor = true;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && characterBody != null)
            {
                characterBody.isSprinting = true;
            }

            // Apply momentum damping for ball-like deceleration
            if (isAuthority && characterMotor != null && characterMotor.isGrounded)
            {
                Vector3 velocity = characterMotor.velocity;
                velocity.x *= momentumDamping;
                velocity.z *= momentumDamping;
                characterMotor.velocity = velocity;
            }

            // Regenerate ammo for all weapons while in ball form
            if (NetworkServer.active && controller != null)
            {
                controller.RegenAllAmmo(GetDeltaTime());
            }
        }

        public override void OnExit()
        {
            AkSoundEngine.StopPlayingID(loopSoundID);
            Util.PlaySound(exitSoundString, gameObject);

            if (characterBody != null)
            {
                characterBody.baseMoveSpeed = originalMoveSpeed;
                characterBody.baseAcceleration = originalAcceleration;
                characterBody.statsDirty = true;
                characterBody.isSprinting = false;
            }

            if (characterMotor != null)
            {
                characterMotor.airControl = originalAirControl;
            }

            if (NetworkServer.active && characterBody != null)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
            }

            if (NetworkServer.active && controller != null)
            {
                controller.ResetRegenAccumulators();
            }

            if (modelAnimator != null)
            {
                modelAnimator.SetFloat(aimWeightHash, 1f);
            }

            if (modelLocator != null)
            {
                modelLocator.normalizeToFloor = false;
            }

            PlayAnimation("Body", "BallModeExit");

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
