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
            //Util.PlayAttackSpeedSound(MinigunSpinDown.sound, base.gameObject, this.attackSpeedStat);
            //base.GetModelAnimator().SetBool(MinigunSpinDown.WeaponIsReadyParamHash, false);
        }

        // Token: 0x060018B0 RID: 6320 RVA: 0x00072D81 File Offset: 0x00070F81
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
