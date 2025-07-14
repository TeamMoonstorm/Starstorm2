using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Mimic.Weapon
{
    public class MimicMinigunExit : MimicMinigunState
    {
        public static float baseDuration;

        public static string mecanimPeramater;

        private float duration;
        private bool hasFired;
        //private Transform muzzle;
        private Animator animator;

        private float originalMoveSpeed;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayCrossfade("Gesture, Override", "MinigunExit", "MinigunFire.playbackRate", duration, 0.05f);

            if (fireVFXInstanceLeft)
            {
                EntityState.Destroy(fireVFXInstanceLeft);
            }

            if (fireVFXInstanceRight)
            {
                EntityState.Destroy(fireVFXInstanceRight);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
