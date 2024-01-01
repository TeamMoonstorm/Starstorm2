using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Agarthan
{
    public class Spit : BaseSkillState
    {
        public static float baseDuration;
        public static float baseDurToSpit;
        private float duration;
        private float durToSpit;
        private bool isSpitting = false;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            durToSpit = baseDurToSpit / attackSpeedStat;
            PlayAnimation("Gesture, Override", "StandUp");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isSpitting && Time.fixedDeltaTime >= durToSpit)
            {
                isSpitting = true;
                PlayAnimation("Gesture, Override", "StandSpit");
            }
            if (Time.fixedDeltaTime >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayAnimation("Gesture, Override", "SpitExit");
        }
    }
}
