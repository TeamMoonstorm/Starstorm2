using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using EntityStates;
namespace SS2
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/SS2ReloadSkillDef2")]
    public class SS2ReloadSkillDef2 : SkillDef
    {
        /*protected class InstanceData : SS2ReloadSkillDef.BaseSkillInstanceData
		{
            EntityStateMachine weaponStateMachine;
            public void OnInventoryChanged()
            {
                skillSlot.RecalculateValues();
            }
        }

        public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
        {
            InstanceData instanceData = new InstanceData();
            weaponStateMachine = EntityStateMachine.FindByCustomName(skillSlot.gameObject, "Weapon");
            skillSlot.characterBody.onInventoryChanged += instanceData.OnInventoryChanged;
            return instanceData;
        }

        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
        {
            if (weaponStateMachine)
        }

        public override void OnUnassigned(GenericSkill skillSlot)
        {
            skillSlot.characterBody.onInventoryChanged -= ((InstanceData)skillSlot.skillInstanceData).OnInventoryChanged;
        }*/

        /*public override int GetRechargeStock(GenericSkill skillSlot)
        {
            return GetMaxStock(skillSlot) + skillSlot.bonusStockFromBody;
        }*/
    }
}
