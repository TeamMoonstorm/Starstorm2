using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Chirr.Wings
{
    public class WingsOn : BaseState
    {
        private Transform hoverEffect;
        public static float hoverVelocity = -1f;
        public static float hoverAcceleration = 60f;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayCrossfade("Wings, Override", "Glide", "Primary.playbackRate", 1, 0.05f);

            base.characterBody.bodyFlags |= RoR2.CharacterBody.BodyFlags.SprintAnyDirection;
            //find & enable hover effect
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.isSprinting = true;
            if (isAuthority)
            {
                float fallVelocity = characterMotor.velocity.y;
                fallVelocity = Mathf.MoveTowards(fallVelocity, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
                characterMotor.velocity = new Vector3(characterMotor.velocity.x, fallVelocity, characterMotor.velocity.z);

                if(base.characterMotor && base.inputBank)
                {
                    bool isDescending = inputBank.jump.down && characterMotor.velocity.y < 0f && !characterMotor.isGrounded;
                    if (!isDescending)
                        this.outer.SetNextStateToMain();

                }
                
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            base.characterBody.bodyFlags &= ~RoR2.CharacterBody.BodyFlags.SprintAnyDirection;

            PlayCrossfade("Wings, Override", "Idle", "Primary.playbackRate", 1, 0.05f);
            //find & disable hovereffect
        }
    }
}
