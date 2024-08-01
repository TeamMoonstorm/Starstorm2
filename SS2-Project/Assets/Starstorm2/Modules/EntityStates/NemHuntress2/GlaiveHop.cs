using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.NemHuntress2
{
    public class GlaiveHop : BaseSkillState
    {
        public float charge;

        
        public static float hopVelocity;
        [SerializeField]
        public static float baseDuration;

        //private float duration;
        //private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            //animator = GetModelAnimator();
            //duration = baseDuration / attackSpeedStat;
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
                SmallHop(characterMotor, hopVelocity * .4f);
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