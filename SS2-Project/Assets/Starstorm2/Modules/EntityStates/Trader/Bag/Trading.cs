using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace EntityStates.Trader.Bag
{
    public class Trading : BagBaseState
    {
        public static float duration;
        public static string enterSoundString;
        public static string exitSoundString;
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
            Util.PlaySound(enterSoundString, gameObject);
            //vfx, sfx, etc
        }

        public override void OnExit()
        {
            //vfx, sfx, etc
            Util.PlaySound(exitSoundString, gameObject);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
            {
                outer.SetNextState(new TradeToIdle());
            }
        }
    }
}
