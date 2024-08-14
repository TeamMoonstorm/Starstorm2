using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using EntityStates;
namespace SS2
{
    [CreateAssetMenu(menuName = "Starstorm2/SkillDef/SS2ReloadSkillDef")]
	public class SS2ReloadSkillDef : SkillDef 
	{
		[Header("Reload Parameters")]
		[Tooltip("The reload state to go into, when stock is less than max.")]
		public SerializableEntityStateType reloadState;

		[Tooltip("The priority of this reload state.")]
		public InterruptPriority reloadInterruptPriority = InterruptPriority.Skill;

		[Tooltip("The amount of time to wait between when we COULD reload, and when we actually start")]
		public float graceDuration;
		protected class InstanceData : SkillDef.BaseSkillInstanceData
		{
			public int currentStock;
			public float graceStopwatch;
			public float delayStopwatch;
		}

		public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
		{
			return new SS2ReloadSkillDef.InstanceData();
		}

		public override void OnFixedUpdate([NotNull] GenericSkill skillSlot)
		{
			base.OnFixedUpdate(skillSlot);
			SS2ReloadSkillDef.InstanceData instanceData = (SS2ReloadSkillDef.InstanceData)skillSlot.skillInstanceData;
			instanceData.currentStock = skillSlot.stock;
			if (instanceData.currentStock < (this.GetMaxStock(skillSlot) + skillSlot.bonusStockFromBody)) //lets reload skill defs account for stock changes properly 
			{
				if (skillSlot.stateMachine && !skillSlot.stateMachine.HasPendingState() && skillSlot.stateMachine.CanInterruptState(this.reloadInterruptPriority))
				{
					instanceData.graceStopwatch += Time.fixedDeltaTime;
					instanceData.delayStopwatch -= Time.fixedDeltaTime;
					if ((instanceData.graceStopwatch >= this.graceDuration || instanceData.currentStock == 0) && instanceData.delayStopwatch <= 0)
					{
						skillSlot.stateMachine.SetNextState(EntityStateCatalog.InstantiateState(this.reloadState));
						return;
					}
				}
				else
				{
					instanceData.graceStopwatch = 0f;
				}
			}

		}
		public override void OnExecute([NotNull] GenericSkill skillSlot)
		{
			base.OnExecute(skillSlot);
			((SS2ReloadSkillDef.InstanceData)skillSlot.skillInstanceData).currentStock = skillSlot.stock;
		}

		public void SetDelayTimer([NotNull] GenericSkill skillSlot, float time)
        {
			SS2ReloadSkillDef.InstanceData instanceData = (SS2ReloadSkillDef.InstanceData)skillSlot.skillInstanceData;
			instanceData.delayStopwatch = time;
		}
	}
}
