using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Invaders.Green
{
    public class FireRay : GenericProjectileBaseState
    {
        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            if (GetModelAnimator())
            {
                //PlayAnimation();
            }
        }
    }
}
