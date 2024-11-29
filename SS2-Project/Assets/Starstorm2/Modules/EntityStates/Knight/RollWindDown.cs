using EntityStates.Generic;
using MSU.Config;
using SS2;
using UnityEngine;

namespace EntityStates.Knight
{
    public class RollWindDown : BaseWindDownState
    {
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testWindDown = 0.1f;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = testWindDown;
        }
    }
}