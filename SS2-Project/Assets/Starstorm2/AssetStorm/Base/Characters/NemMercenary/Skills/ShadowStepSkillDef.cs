using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Skills;
using JetBrains.Annotations;
using RoR2;
using EntityStates;
using Moonstorm.Starstorm2.Components;
namespace Moonstorm.Starstorm2.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Starstorm2/SkillDef/ShadowStepSkillDef")]

	public class ShadowStepSkillDef : SkillDef
	{
		//only activatable with target. always activatable when target is a hologram

		public Sprite hologramOverrideIcon;
		public SerializableEntityStateType hologramOverrideState;
		public string hologramOverrideStateMachine = "Body";
		public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
		{
			return new ShadowStepSkillDef.InstanceData
			{
				tracker = skillSlot.GetComponent<NemMercTracker>()
			};
		}

		
        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
			EntityStateMachine body = EntityStateMachine.FindByCustomName(skillSlot.gameObject, hologramOverrideStateMachine);

			if(body && TargetIsHologram(skillSlot))
            {
				body.SetInterruptState(this.InstantiateNextState(skillSlot), this.interruptPriority);
			}
			else
			{
				skillSlot.stateMachine.SetInterruptState(this.InstantiateNextState(skillSlot), this.interruptPriority);
			}
				

			if (this.cancelSprintingOnActivation)
			{
				skillSlot.characterBody.isSprinting = false;
			}
			skillSlot.stock -= this.stockToConsume;
			if (this.resetCooldownTimerOnUse)
			{
				skillSlot.rechargeStopwatch = 0f;
			}
			if (skillSlot.characterBody)
			{
				skillSlot.characterBody.OnSkillActivated(skillSlot);
			}

			if (TargetIsHologram(skillSlot))
            {
				skillSlot.stock += base.stockToConsume;
            }
        }

        public override EntityState InstantiateNextState([NotNull] GenericSkill skillSlot)
        {
			SerializableEntityStateType state = TargetIsHologram(skillSlot) ? hologramOverrideState : this.activationState;
			EntityState entityState = EntityStateCatalog.InstantiateState(state);
			ISkillState skillState;
			if ((skillState = (entityState as ISkillState)) != null)
			{
				skillState.activatorSkillSlot = skillSlot;
			}
			return entityState;
		}
        public override Sprite GetCurrentIcon([NotNull] GenericSkill skillSlot)
        {
			return TargetIsHologram(skillSlot) ? hologramOverrideIcon : base.GetCurrentIcon(skillSlot);
        }
        public static bool HasTarget([NotNull] GenericSkill skillSlot)
		{
			NemMercTracker tracker = ((ShadowStepSkillDef.InstanceData)skillSlot.skillInstanceData).tracker;
			return (tracker != null) ? tracker.GetTrackingTarget() : null;
		}
		public static bool TargetIsHologram([NotNull] GenericSkill skillSlot)
        {
			NemMercTracker tracker = ((ShadowStepSkillDef.InstanceData)skillSlot.skillInstanceData).tracker;
			return (tracker != null) ? tracker.IsTargetHologram() : false;
		}
		public override bool CanExecute([NotNull] GenericSkill skillSlot)
		{
			return (ShadowStepSkillDef.HasTarget(skillSlot) && base.CanExecute(skillSlot)) || ShadowStepSkillDef.TargetIsHologram(skillSlot);
		}
		public override bool IsReady([NotNull] GenericSkill skillSlot)
		{
			return (ShadowStepSkillDef.HasTarget(skillSlot) && base.IsReady(skillSlot)) || ShadowStepSkillDef.TargetIsHologram(skillSlot);
		}

		public class InstanceData : SkillDef.BaseSkillInstanceData
		{
			public NemMercTracker tracker;
		}

	}
}
