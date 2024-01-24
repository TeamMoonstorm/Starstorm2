using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
namespace EntityStates.Chirr.Wings
{
    public class WingsOn : BaseState
    {
        private Transform hoverEffect;
        public static float hoverVelocity = -3f;
        public static float hoverAcceleration = 50f;

        private uint soundId;

        private float graceTimer = .6f; // lets us stay in the state for a moment if we move upwards

        private Animator animator;

        private bool isToggle;
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Body", "EnterHover");
            this.animator = base.GetModelAnimator();
            if(this.animator)
            {
                this.animator.SetBool("inHover", true);
            }
            Util.PlaySound("ChirrSprintStart", base.gameObject); // loop should be in start event but idc
            this.soundId = Util.PlaySound("ChirrSprintLoop", base.gameObject);
            isToggle = Moonstorm.Starstorm2.Survivors.Chirr.toggleHover;
            base.characterBody.bodyFlags |= RoR2.CharacterBody.BodyFlags.SprintAnyDirection;
            //find & enable hover effect
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.isSprinting = true;
            if (isAuthority && base.characterMotor && base.inputBank)
            {
                float fallVelocity = characterMotor.velocity.y;
                fallVelocity = Mathf.MoveTowards(fallVelocity, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
                fallVelocity = Mathf.Max(characterMotor.velocity.y, fallVelocity); // let us get small boosts upwards
                characterMotor.velocity = new Vector3(characterMotor.velocity.x, fallVelocity, characterMotor.velocity.z);
                if (characterMotor.velocity.y >= 0f) this.graceTimer -= Time.fixedDeltaTime;
                else this.graceTimer = .6f;

                //if toggling is true, jump doesnt need to be down in order to stay in hover
                bool jumpInput = inputBank.jump.down || this.isToggle;
                bool isDescending = jumpInput && this.graceTimer > 0f && !characterMotor.isGrounded;

                //if toggling is true, end hovering when jump is pressed again
                bool justToggled = this.isToggle && inputBank.jump.justPressed;

                if (!isDescending || justToggled)
                {
                    this.outer.SetNextStateToMain();                   
                }

                        

                
                
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            base.characterBody.bodyFlags &= ~RoR2.CharacterBody.BodyFlags.SprintAnyDirection;
            AkSoundEngine.StopPlayingID(this.soundId);
            Util.PlaySound("ChirrSprintStop", base.gameObject);
            if (this.animator)
            {
                this.animator.SetBool("inHover", false);
            }
            //find & disable hovereffect
        }
    }
}
