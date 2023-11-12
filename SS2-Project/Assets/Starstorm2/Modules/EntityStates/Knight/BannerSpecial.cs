using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerSpecial : BaseState
{
    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override InterruptPriority GetMinimumInterruptPriority()
    {
        return InterruptPriority.PrioritySkill;
    }
}
}
