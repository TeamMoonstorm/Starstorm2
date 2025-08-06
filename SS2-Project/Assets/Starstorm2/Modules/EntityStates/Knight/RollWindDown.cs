using EntityStates.Generic;
using MSU.Config;
using SS2;
using UnityEngine;

namespace EntityStates.Knight
{
    public class RollWindDown : BaseWindDownState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            duration = Roll.testWindDown;
        }
    }
}