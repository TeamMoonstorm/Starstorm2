using JetBrains.Annotations;
using MSU;
using RoR2;
using RoR2.Skills;
using SS2.Components;

using UnityEngine;

namespace SS2.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/ToolbotSelfRepairSkillDef")]
    public class SelfRepairSkillDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            skillSlot.characterBody.gameObject.AddComponent<SelfRepairController>();

            SelfRepairController selfRepairMeter = skillSlot.characterBody.gameObject.EnsureComponent<SelfRepairController>();
            //selfRepairMeter.repairOverlayPrefab = SS2.Survivors.Toolbot.RepairOverlayPrefab;
            
            return new SelfRepairSkillDef.InstanceData
            {
                selfRepairMeter = selfRepairMeter
            };
        }

        public class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public SelfRepairController selfRepairMeter;
        }
    }
}
