using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using SS2.Components;
using UnityEngine;

namespace SS2.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/NemToolbotPrimarySkillDef")]
    public class NemToolbotPrimarySkillDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            NemToolbotController controller = skillSlot.GetComponent<NemToolbotController>();
            if (!controller)
                SS2Log.Error("NemToolbotPrimarySkillDef: Missing NemToolbotController on " + skillSlot.gameObject.name);

            return new InstanceData { controller = controller };
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            NemToolbotController controller = ((InstanceData)skillSlot.skillInstanceData).controller;
            return controller && controller.HasAmmo(controller.currentWeapon) && base.IsReady(skillSlot);
        }

        private class InstanceData : BaseSkillInstanceData
        {
            public NemToolbotController controller;
        }
    }
}
