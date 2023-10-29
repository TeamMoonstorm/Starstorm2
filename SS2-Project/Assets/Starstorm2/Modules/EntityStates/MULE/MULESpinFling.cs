using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.MULE
{
    public class MULESpinFling : BaseSkillState
    {
        public static float baseDamageCoefficient;
        public static float minDuration;
        public static float maxDuration;
        private float duration;
        public float charge;

        public static float airControl;
        private float previousAirControl;
        public static float minUpwardVelocity;
        public static float maxUpwardVelocity;
        private float upwardVelocity;
        public static float minForwardVelocity;
        public static float maxForwardVelocity;
        private float forwardVelocity;
        public static float knockbackForce;
        public static float minimumY;
        public static float aimVelocity;

        private Vector3 dashVector = Vector3.zero;
        private bool hasFlung = false;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = Util.Remap(charge, 0f, 1f, minDuration, maxDuration) / attackSpeedStat;
            characterBody.hideCrosshair = true;
            //PlayAnimation();
            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            previousAirControl = characterMotor.airControl;
            characterMotor.airControl = airControl;
            Vector3 direction = GetAimRay().direction;

            upwardVelocity = Util.Remap(charge, 0f, 1f, minUpwardVelocity, maxUpwardVelocity);
            forwardVelocity = Util.Remap(charge, 0f, 1f, minForwardVelocity, maxForwardVelocity);

            if (isAuthority)
            {
                //characterDirection.forward = dashVector;
                characterBody.isSprinting = true;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 b = Vector3.up * upwardVelocity;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = a + b + b2;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
                FixedUpdateAuthority();
        }

        public void FixedUpdateAuthority()
        {
            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            characterBody.isSprinting = false;
            characterMotor.airControl = previousAirControl;
            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.OnExit();
        }
    }
}
