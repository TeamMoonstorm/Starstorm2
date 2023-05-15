using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace EntityStates.Trader.Bag
{
    public class WaitToBeginTrade : BagBaseState
    {
        public static float duration;
        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
                outer.SetNextState(new Trading());
        }
    }
}
