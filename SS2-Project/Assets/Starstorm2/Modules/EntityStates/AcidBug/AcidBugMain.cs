using RoR2;
using UnityEngine;

namespace EntityStates.AcidBug
{
    public class AcidBugMain : FlyState
    {
        private static float turnSpeed = 360f; // FUCK RIGIDBODYDIRECTION WHAT TH EFUCK
        private static float turnSmoothTime = 0.1f;

        private static float minY = -0.6f;
        private static float maxY = 0.6f;

        private Vector3 targetRotationVelocity;
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                Vector3 currentDirection = transform.forward;
                Vector3 inputDirection = inputBank.aimDirection;
                inputDirection.y = Mathf.Clamp(inputDirection.y, minY, maxY);
                inputDirection = inputDirection.normalized;
                Vector3 newDirection = Vector3.SmoothDamp(currentDirection, inputDirection, ref targetRotationVelocity, turnSmoothTime, turnSpeed);
                rigidbody.MoveRotation(Util.QuaternionSafeLookRotation(newDirection));
                rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}
