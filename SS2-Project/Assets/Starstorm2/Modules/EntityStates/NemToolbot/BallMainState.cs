using RoR2;
using SS2;
using SS2.Components;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    public class BallMainState : BaseCharacterMain
    {
        public static float moveSpeedMultiplier = 1.6f;
        public static float ballAirControl = 0.15f;
        public static float accelerationMultiplier = 0.6f;
        public static float momentumDamping = 0.98f;

        public static string enterSoundString = "";
        public static string loopSoundString = "";
        public static string exitSoundString = "";

        public override void OnEnter()
        {
            base.OnEnter();
            if (!gameObject.TryGetComponent(out NemToolbotController controller) || !controller.SetBallForm(true))
            {
                SS2Log.Error("NemToolbot BallMainState: Missing or unconfigured NemToolbotController.");
                return;
            }
            // PlayCrossfade("Body", "BallModeEnter", 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority)
                return;

            characterBody.isSprinting = true;
            if (characterMotor.isGrounded)
            {
                float damping = Mathf.Pow(momentumDamping, GetDeltaTime() / 0.02f);
                Vector3 velocity = characterMotor.velocity;
                velocity.x *= damping;
                velocity.z *= damping;
                characterMotor.velocity = velocity;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
