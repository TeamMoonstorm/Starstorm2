using SS2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.DUT

{
    public class Drift : BaseSkillState
    {
        public static float dashDuration = 0.3f;
        public static float releaseDuration = 0.2f;
        public static float driftDuration = 2.5f;
        public static float initalSpeedCoefficient = 4.5f;
        public static float finalSpeedCoefficient = 1.5f;
        public static float turnSpeed = 360f;
        public static float upThing = 0.67f;
        public static float FOV = -1f;


        private float dashSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        //private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active) characterBody.AddBuff(SS2Content.Buffs.bdDUTDrift);

            if (isAuthority && inputBank && characterDirection)
                forwardDirection = ((inputBank.moveVector == Vector3.zero) ? characterDirection.forward : inputBank.moveVector).normalized;

            if (characterMotor && characterDirection)
            {
                // min is 0, max is rollSpeed
                float y = Mathf.Min(Mathf.Max(characterMotor.velocity.y, 0), dashSpeed) * upThing;
                characterMotor.velocity = forwardDirection * dashSpeed;
                characterMotor.velocity.y = y;
            }

            characterDirection.turnSpeed = turnSpeed;

            Vector3 velocity = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - velocity;
        }

        private void RecalcDashSpeed()
        {
            dashSpeed = moveSpeedStat * Mathf.Lerp(initalSpeedCoefficient, finalSpeedCoefficient, fixedAge / dashSpeed);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge <= dashDuration)
            {
                RecalcDashSpeed();

                if (isAuthority)
                {
                    Vector3 normalized = (transform.position - previousPosition).normalized;
                    if (characterMotor && characterDirection && normalized != Vector3.zero)
                    {
                        Vector3 vector = normalized * dashSpeed;
                        float y = vector.y;
                        vector.y = 0f;
                        float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                        vector = forwardDirection * d;
                        vector.y += Mathf.Max(y, 0f);
                        characterMotor.velocity = vector;

                        Vector3 rhs = inputBank ? characterDirection.forward : forwardDirection;
                        Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
                        float num = Vector3.Dot(forwardDirection, rhs);
                        float num2 = Vector3.Dot(forwardDirection, rhs2);
                    }

                    previousPosition = transform.position;
                }
            }
            else
            {
                if (!inputBank.skill3.down || fixedAge > driftDuration)
                {
                    outer.SetNextStateToMain();
                }
            }

            characterBody.isSprinting = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(SS2Content.Buffs.bdDUTDrift);
            }
            characterDirection.turnSpeed = 720f;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
