using System;
using EntityStates;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2.Skills
{
	[CreateAssetMenu(menuName = "Starstorm2/SkillDef/CleanShotSkillDef")]

	//Same as ReloadSkillDef but gives options on behavior if you fire the ability on an empty stock
	public class CleanShotSkillDef : ReloadSkillDef
	{
		public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
		{
			return new CleanShotSkillDef.InstanceData();
		}
		public override EntityState InstantiateNextState([NotNull] GenericSkill skillSlot)
		{
			SerializableEntityStateType state = skillSlot.stock == 0 ? emptyOverrideState: activationState;
			EntityState entityState = EntityStateCatalog.InstantiateState(state.stateType);
			ISkillState skillState;
			if ((skillState = (entityState as ISkillState)) != null)
			{
				skillState.activatorSkillSlot = skillSlot;
			}
			return entityState;
		}

        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
        {
			CleanShotSkillDef.InstanceData instanceData = (CleanShotSkillDef.InstanceData)skillSlot.skillInstanceData;
			if (skillSlot.characterBody.hasCloakBuff)
			{
				if (!instanceData.wasCloaked)
				{
					skillSlot.Reset();
					skillSlot.stock = skillSlot.maxStock;
				}
			}
			instanceData.wasCloaked = skillSlot.characterBody.hasCloakBuff;

			base.OnFixedUpdate(skillSlot, deltaTime);
        }

        [Tooltip("The state to enter when this skill is activated on an empty stock.")]
		public SerializableEntityStateType emptyOverrideState;

		protected class InstanceData : ReloadSkillDef.InstanceData
		{
			public bool wasCloaked;
		}
	}
}
