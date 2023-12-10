using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Chirr.Wings
{
    public class BoostUp : BaseState
    {
        private Transform hoverEffect;

        public float boostVelocity;
        public static float boostAcceleration = 130f;
        public static float baseDuration = .7f;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration;// / base.moveSpeedStat;
            PlayCrossfade("Wings, Override", "Glide", "Primary.playbackRate", 1, 0.05f);
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
            PlayCrossfade("Wings, Override", "Idle", "Primary.playbackRate", 1, 0.05f);
            //find & disable hovereffect
        }
    }
}
