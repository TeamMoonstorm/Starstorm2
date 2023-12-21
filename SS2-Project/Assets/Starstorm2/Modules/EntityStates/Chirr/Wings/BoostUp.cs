using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
namespace EntityStates.Chirr.Wings
{
    public class BoostUp : BaseState
    {
        private Transform hoverEffect;

        public float boostVelocity;
        public static float boostAcceleration = 90f;
        public static float baseDuration = 1f;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration;// / base.moveSpeedStat;
            Util.PlaySound("ChirrGrabFlyUp", base.gameObject);
            if(base.characterMotor)
            {
                base.characterMotor.Motor.ForceUnground();
            }
            //find & enable hover effect
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                if(base.isGrounded)
                    base.characterMotor.Motor.ForceUnground(); // :(
                
                float fallVelocity = characterMotor.velocity.y;
                float acceleration = Mathf.Lerp(boostAcceleration, 0f, base.fixedAge / this.duration);
                fallVelocity += acceleration * Time.fixedDeltaTime;
                characterMotor.velocity = new Vector3(characterMotor.velocity.x, fallVelocity, characterMotor.velocity.z);
            }

            if(base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            //find & disable hovereffect
        }
    }
}
