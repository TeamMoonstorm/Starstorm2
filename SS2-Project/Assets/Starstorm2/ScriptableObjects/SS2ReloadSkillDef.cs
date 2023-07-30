using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Starstorm2.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Starstorm2/SkillDef/SS2ReloadSkillDef")]
	public class SS2ReloadSkillDef : ReloadSkillDef
	{
		public override void OnFixedUpdate([NotNull] GenericSkill skillSlot)
		{
			base.OnFixedUpdate(skillSlot);
			ReloadSkillDef.InstanceData instanceData = (ReloadSkillDef.InstanceData)skillSlot.skillInstanceData;
			instanceData.currentStock = skillSlot.stock;
			if (instanceData.currentStock < (this.GetMaxStock(skillSlot) + skillSlot.bonusStockFromBody)) //lets reload skill defs account for stock changes properly 
			{
				if (skillSlot.stateMachine && !skillSlot.stateMachine.HasPendingState() && skillSlot.stateMachine.CanInterruptState(this.reloadInterruptPriority))
				{
					instanceData.graceStopwatch += Time.fixedDeltaTime;
					if (instanceData.graceStopwatch >= this.graceDuration || instanceData.currentStock == 0)
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
	}
}
