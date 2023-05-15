using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Trader.Bag
{
    public class Idle : BagBaseState
    {
        protected override bool enableInteraction
        {
            get
            {
                Debug.Log("returning true");
                return true;
            }
        }
        public override void OnEnter()
        {
            Debug.Log("idle bag state");
        }
    }
}
