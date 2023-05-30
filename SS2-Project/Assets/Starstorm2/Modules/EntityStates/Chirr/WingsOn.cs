using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Chirr
{
    public class WingsOn : BaseState
    {
        private Transform hoverEffect;
        public static float hoverVelocity;
        public static float hoverAcceleration;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayCrossfade("Wings, Override", "Glide", "Primary.playbackRate", 1, 0.05f);
            //find & enable hover effect
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                float fallVelocity = characterMotor.velocity.y;
                fallVelocity = Mathf.MoveTowards(fallVelocity, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
                characterMotor.velocity = new Vector3(characterMotor.velocity.x, fallVelocity, characterMotor.velocity.z);
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
