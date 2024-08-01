using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.NemHuntress2
{
    public class GlaiveHop : BaseSkillState
    {
        public float charge;

        [SerializeField]
        public static float hopVelocity;
        [SerializeField]
        public static float baseDuration;

        //private float duration; //.75f
        //private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            //animator = GetModelAnimator();
            //duration = baseDuration / attackSpeedStat;
            //fireDuration = 0.85f * duration;
            CastHop();
        }

        private void CastHop()
        {
            if (characterMotor.isGrounded)
            {
                SmallHop(characterMotor, hopVelocity);
            }
            else
            {
                SmallHop(characterMotor, hopVelocity * .5f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= baseDuration && base.isAuthority)
            {
                GlaiveThrow glaiveThrow = new GlaiveThrow();
                outer.SetNextState(glaiveThrow);
                return;
            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

    }
}