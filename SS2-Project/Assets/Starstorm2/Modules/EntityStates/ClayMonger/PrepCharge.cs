using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.ClayMonger
{
    public class PrepCharge : BaseState
    {
        public static float stateDuration;

        private float _duration;

        public override void OnEnter()
        {
            base.OnEnter();
            _duration = stateDuration / attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge > _duration)
            {
                outer.SetNextState(new Charge());
            }
        }
    }
}