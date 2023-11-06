using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSwap : BaseState
{
    private GenericSkill originalPrimarySkill;
    private GenericSkill originalSecondarySkill;
    private GenericSkill originalUtilitySkill;
    private GenericSkill originalSpecialSkill;

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
