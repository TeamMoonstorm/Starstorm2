using UnityEngine;

namespace EntityStates.Knight
{
    public class Flurry : BaseKnightMeleeAttack
    {
        public float totalDuration { get; set; }
        private bool swipeDown;

        public override void PlayAttackAnimation()
        {
            PlayAnimation("FullBody, Override", "FlurrySwipe" + (swipeDown? "Down": "Up"));
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterMotor.velocity = Vector3.zero;
        }

        protected override void SetNextState()
        {
            if(totalDuration > 0)
            {
                outer.SetNextState(new Flurry { totalDuration = totalDuration - fixedAge, swipeDown = !swipeDown});
            }
            else
            {
                outer.SetNextStateToMain();
            }
        }

    }
}