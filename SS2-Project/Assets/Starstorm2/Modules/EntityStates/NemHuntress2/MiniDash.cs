using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemHuntress2
{
    public class MiniDash : Huntress.MiniBlinkState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            /*characterBody.skillLocator.primary.stock += 2;
            if (characterBody.skillLocator.primary.stock > 7)
                characterBody.skillLocator.primary.stock = 7;*/
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
