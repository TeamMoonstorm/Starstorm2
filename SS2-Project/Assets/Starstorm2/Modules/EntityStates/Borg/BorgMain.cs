using UnityEngine;

namespace EntityStates.Borg
{
    public class BorgMain : GenericCharacterMain
    {
        private float hoverVelocity = -1.1f;
        private float hoverAcceleration = 25f;

        public override void OnEnter()
        {
            base.OnEnter();

        }

        public override void OnExit()
        {

            base.OnExit();
        }

        public override void ProcessJump()
        {
            base.ProcessJump();
            if (hasCharacterMotor && hasInputBank && isAuthority)
            {
                bool hoverInput = inputBank.jump.down && characterMotor.velocity.y < 0f && !characterMotor.isGrounded;

                if (isAuthority && hoverInput)
                {
                    float num = characterMotor.velocity.y;
                    num = Mathf.MoveTowards(num, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
                    characterMotor.velocity = new Vector3(characterMotor.velocity.x, num, characterMotor.velocity.z);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // rest idle!!
            //if (animator) animator.SetBool("inCombat", (!base.characterBody.outOfCombat || !base.characterBody.outOfDanger));
        }
    }
}