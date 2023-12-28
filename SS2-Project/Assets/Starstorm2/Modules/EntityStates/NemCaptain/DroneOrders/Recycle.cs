using Moonstorm.Starstorm2.Components;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class Recycle : BaseSkillState
    {
        private NemCaptainController ncc;
        private float dur = 0.1f;
        public override void OnEnter()
        {
            base.OnEnter();
            ncc = characterBody.GetComponent<NemCaptainController>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= dur && isAuthority)
            {
                outer.SetNextState(new ForcedCooldown());
            }
        }

        public override void OnExit()
        {
            activatorSkillSlot.SetSkillOverride(gameObject, ncc.GetRandomSkillDefFromDeck(), GenericSkill.SkillOverridePriority.Replacement);
            EntityStateMachine esm = EntityStateMachine.FindByCustomName(gameObject, "Skillswap");
            if (esm)
                esm.SetNextStateToMain();
            base.OnExit();
        }
    }
}
