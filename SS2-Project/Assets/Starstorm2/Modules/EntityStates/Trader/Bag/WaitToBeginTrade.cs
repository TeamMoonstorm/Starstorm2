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

        public override void OnEnter()
        {
            base.OnEnter();
            if (isAuthority)
                characterBody.SetBuffCount(Moonstorm.Starstorm2.SS2Content.Buffs.bdHiddenSlow20.buffIndex, 10);
            PlayCrossfade("Body", "Scavenge", "Scavenge.playbackRate", duration + Trading.duration + TradeToIdle.duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
                outer.SetNextState(new Trading());
        }
    }
}
