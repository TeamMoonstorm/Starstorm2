using RoR2.Skills;
using UnityEngine;
using JetBrains.Annotations;
using RoR2;
using SS2.Components;

namespace SS2
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/HeatSkillDef")]
    public class HeatSkillDef : SkillDef
    {
        [Tooltip("The amount of heat required for this skill.")]
        public float heatValue = 0;
        [Tooltip("The amount of heat consumed by this skill.")]
        public float heatConsumed = 0;
        [Tooltip("If the skill can be casted when there is insufficient heat.")]
        public bool canCastIfLowHeat = false;
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                pc = skillSlot.GetComponent<PyroController>()
            };
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            PyroController pc = ((InstanceData)skillSlot.skillInstanceData).pc;
            return (heatValue <= pc.heat && base.CanExecute(skillSlot));
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            PyroController pc = ((InstanceData)skillSlot.skillInstanceData).pc;
            return base.IsReady(skillSlot) && (heatValue <= pc.heat);
        }

        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            base.OnExecute(skillSlot);
            PyroController ncc = ((InstanceData)skillSlot.skillInstanceData).pc;
            ncc.AddHeat(heatConsumed);
        }

        protected class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public PyroController pc;
        }
    }
}
