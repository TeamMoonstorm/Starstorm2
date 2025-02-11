using JetBrains.Annotations;
using RoR2;
using SS2.Components;
using RoR2.Skills;
using UnityEngine;
using EntityStates;
namespace SS2
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/CommandSkillDef")]
    public class CommandSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
		{
            public GenericSkill skillSlot;
            public NemCaptainController nemCaptainController;
            public void OnInventoryChanged()
            {
                skillSlot.RecalculateValues();
                if ((bool)nemCaptainController)
                {
                    nemCaptainController.bonusMaxStressStacks = (int)Mathf.Max(skillSlot.maxStock - 1, 0);
                }
            }
        }

        public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
        {
            InstanceData instanceData = new InstanceData 
            {
                nemCaptainController = skillSlot.GetComponent<NemCaptainController>()
            };
            Debug.Log("OnAssign");
            instanceData.skillSlot = skillSlot;
            skillSlot.characterBody.onInventoryChanged += instanceData.OnInventoryChanged;
            return instanceData;
        }

        public override void OnUnassigned(GenericSkill skillSlot)
        {
            Debug.Log("OnUnassign");
            skillSlot.characterBody.onInventoryChanged -= ((InstanceData)skillSlot.skillInstanceData).OnInventoryChanged;
        }
    }
}
