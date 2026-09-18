using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Overseer.Weapon
{
    public class FireEyeBolt : GenericProjectileBaseState
    {
        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            Animator animator = GetModelAnimator();
            if (animator != null)
            {
                PlayAnimation("Gesture, Additive", "FireEyeBolt", "Primary.playbackRate", duration);
            }
        }
    }
}

