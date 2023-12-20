using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;
using RoR2;
using Moonstorm.Starstorm2.Components;

namespace Assets.Starstorm2.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/OverstressedSkillDef")]
    public class OverstressedSkillDef : SkillDef
    {
        [Tooltip("If the skill can be casted while overstressed.")]
        public bool canCastIfOverstressed = false;
        
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                ncc = skillSlot.GetComponent<NemCaptainController>()
            };
        }

        private static bool IsOverstressed([NotNull] GenericSkill skillSlot)
        {
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            return ncc.isOverstressed;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return (!IsOverstressed(skillSlot) || canCastIfOverstressed) && base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && (!IsOverstressed(skillSlot) || canCastIfOverstressed);
        }

        protected class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public NemCaptainController ncc;
        }
    }
}
