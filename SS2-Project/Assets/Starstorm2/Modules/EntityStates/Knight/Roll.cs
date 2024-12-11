using EntityStates;
using MSU.Config;
using RoR2;
using SS2;
using SS2.Orbs;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    public class Roll : BaseKnightDashMelee
    {
        // Movement variables
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testbaseDuration = 0.69f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testminSpeedCoefficient = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testmaxSpeedCoefficient = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testinterruptSpeedCoefficient = 0.0f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testearlyexit = 0.1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testWindDown = 0.1f; // in rollwinddown state

        public override void OnEnter()
        {
            baseDuration = testbaseDuration;
            minSpeedCoefficient = testminSpeedCoefficient;
            maxSpeedCoefficient = testmaxSpeedCoefficient;
            interruptSpeedCoefficient = testinterruptSpeedCoefficient;

            earlyExitTimeFraction = testearlyexit;

            StartAimMode();

            base.OnEnter();
            animator.SetBool("isRolling", true);
            if (isGrounded)
            {
                PlayCrossfade("FullBody, Override", "Roll", "Utility.rate", duration, 0.05f);
            } 
            else
            {
                PlayCrossfade("FullBody, Override", "AirRoll", "Utility.rate", duration, 0.05f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected override void SetNextState()
        {
            outer.SetNextState(new RollWindDown());
        }

        protected override void OnInterrupted()
        {
            return;
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            animator.SetBool("isRolling", false);
            base.OnExit();
            characterMotor.disableAirControlUntilCollision = false;
        }
    }
}