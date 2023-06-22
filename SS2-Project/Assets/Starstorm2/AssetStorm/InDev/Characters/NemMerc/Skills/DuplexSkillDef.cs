using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Skills;
using JetBrains.Annotations;
using RoR2;
using EntityStates;

namespace Moonstorm.Starstorm2.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Starstorm2/SkillDef/DuplexSkillDef")]

	//Activate on press, then again on release

	//this might be the worst possible way of tdoing this
	public class DuplexSkillDef : SkillDef
	{
        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
			return new DuplexSkillDef.InstanceData();
        }
        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot)
		{
			base.OnFixedUpdate(skillSlot);

			//lol       
			if (!skillSlot.characterBody.inputBank.skill1.down && skillSlot.stock % 2 == 1 && skillSlot.CanExecute())
			{
				skillSlot.ExecuteIfReady();
			}

			DuplexSkillDef.InstanceData instanceData = (DuplexSkillDef.InstanceData)skillSlot.skillInstanceData;
			instanceData.currentStock = skillSlot.stock;
			if (instanceData.currentStock == 0)
			{
				if (skillSlot.stateMachine && !skillSlot.stateMachine.HasPendingState() && skillSlot.stateMachine.CanInterruptState(this.reloadInterruptPriority))
				{
					instanceData.graceStopwatch += Time.fixedDeltaTime;
					if (instanceData.graceStopwatch >= this.graceDuration)
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

		[Header("Reload Parameters")]
		[Tooltip("The reload state to go into, when stock is less than max.")]
		public SerializableEntityStateType reloadState;
		[Tooltip("The priority of this reload state.")]
		public InterruptPriority reloadInterruptPriority = InterruptPriority.Skill;
		[Tooltip("The amount of time to wait between when we COULD reload, and when we actually start")]
		public float graceDuration;

		public class InstanceData : SkillDef.BaseSkillInstanceData
		{
			public int currentStock;
			public float graceStopwatch;
		}

	}
}
