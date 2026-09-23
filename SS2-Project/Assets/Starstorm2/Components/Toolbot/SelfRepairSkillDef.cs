using JetBrains.Annotations;
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
            SelfRepairController selfRepairController = skillSlot.GetComponent<SelfRepairController>();
            if (selfRepairController)
            {
                selfRepairController.enabled = true;
            }
            else
            {
                SS2Log.Error("SelfRepairSkillDef: Missing SelfRepairController on " + skillSlot.gameObject.name);
            }

            return new SelfRepairSkillDef.InstanceData
            {
                selfRepairMeter = selfRepairController
            };
        }

        public override void OnUnassigned(GenericSkill skillSlot)
        {
            SelfRepairController selfRepairController = ((InstanceData)skillSlot.skillInstanceData).selfRepairMeter;
            if (selfRepairController)
            {
                selfRepairController.enabled = false;
            }
            base.OnUnassigned(skillSlot);
        }

        public override bool IsReady(GenericSkill skillSlot)
        {
            SelfRepairController selfRepairController = ((InstanceData)skillSlot.skillInstanceData).selfRepairMeter;
            return base.IsReady(skillSlot) && selfRepairController && !selfRepairController.isRepairing
                && selfRepairController.CanRepair(EntityStates.Toolbot.SelfRepair.repairCostPerTick);
        }

        public class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public SelfRepairController selfRepairMeter;
        }
    }
}
