using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;
using RoR2;
using Moonstorm.Starstorm2.Components;

namespace Assets.Starstorm2.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/StressSkillDef")]
    public class StressSkillDef : SkillDef
    {
        [Tooltip("The amount of stress added by this skill.")]
        public float stressValue = 0;
        [Tooltip("If the skill can be casted while overstressed.")]
        public bool canCastIfOverstressed = false;
        [Tooltip("If the skill can be casted when there is insufficient stress.")]
        public bool canCastIfWillOverstress = false;
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
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            return (!IsOverstressed(skillSlot) || canCastIfOverstressed) && (((100 - ncc.stress) > stressValue) || canCastIfWillOverstress) && base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            return base.IsReady(skillSlot) && (!IsOverstressed(skillSlot) || canCastIfOverstressed) && (((100 - ncc.stress) > stressValue) || canCastIfWillOverstress);
        }

        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            base.OnExecute(skillSlot);
            NemCaptainController ncc = ((InstanceData)skillSlot.skillInstanceData).ncc;
            ncc.AddStress(stressValue);
        }

        protected class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public NemCaptainController ncc;
        }
    }
}
