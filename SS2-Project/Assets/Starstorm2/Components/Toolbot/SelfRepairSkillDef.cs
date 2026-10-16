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
            SelfRepairController selfRepairController = skillSlot.characterBody.gameObject.EnsureComponent<SelfRepairController>();

            if (SS2.Survivors.Toolbot.RepairOverlayPrefab != null)
            {
                selfRepairController.repairOverlayPrefab = SS2.Survivors.Toolbot.RepairOverlayPrefab;
            }

            return new SelfRepairSkillDef.InstanceData
            {
                selfRepairMeter = selfRepairController
            };
        }

        public class InstanceData : SkillDef.BaseSkillInstanceData
        {
            public SelfRepairController selfRepairMeter;
        }
    }
}
