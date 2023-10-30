using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Knight
{
    public class LeapInit : BasicMeleeAttack
    {
        public static float swingTimeCoefficient;

        public static float airControl;
        private float previousAirControl;
        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float knockbackForce;
        public static float minimumY;
        public static float aimVelocity;
        public bool swingSide;

        public float totalSwings = 0f;
        public static float baseSwingCount = 4f;
        public float swingCount;


        public override void OnEnter()
        {
            base.OnEnter();
            swingCount = baseSwingCount * attackSpeedStat;
            totalSwings++;
            animator = GetModelAnimator();
            Vector3 direction = GetAimRay().direction;

            if (isAuthority)
            {
                characterBody.isSprinting = true;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 b = Vector3.up * upwardVelocity;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = a + b + b2;
            }
        }

        public override void PlayAnimation()
        {
            string animationStateName = swingSide ? "SwingSword1" : "SwingSword2";
            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.05f);
        }

        public override void OnExit()
        {
            if (totalSwings < baseSwingCount)
            {
                LeapSlash2 nextState = new LeapSlash2();
                nextState.totalSwings = totalSwings;
                nextState.swingCount = swingCount;
                nextState.swingSide = !swingSide;
                outer.SetNextState(nextState);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
