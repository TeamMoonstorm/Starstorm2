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
	[CreateAssetMenu(menuName = "Starstorm2/SkillDef/ChirrTrackingSkillDef")]

	public class ChirrTrackingSkillDef : SkillDef
	{
		public bool isScepter;
		public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
		{
			ChirrFriendTracker tracker = skillSlot.GetComponent<ChirrFriendTracker>();
			if (tracker) tracker.isScepter = this.isScepter;
			return new ChirrTrackingSkillDef.InstanceData
			{
				tracker = tracker
			};
		}
		public static bool HasTarget([NotNull] GenericSkill skillSlot)
		{
			ChirrFriendTracker tracker = ((ChirrTrackingSkillDef.InstanceData)skillSlot.skillInstanceData).tracker;
			return (tracker != null) ? tracker.GetTrackingTarget() : null;
		}
		public override bool CanExecute([NotNull] GenericSkill skillSlot)
		{
			return (ChirrTrackingSkillDef.HasTarget(skillSlot) && base.CanExecute(skillSlot));
		}
		public override bool IsReady([NotNull] GenericSkill skillSlot)
		{
			return (ChirrTrackingSkillDef.HasTarget(skillSlot) && base.IsReady(skillSlot));
		}

		public class InstanceData : SkillDef.BaseSkillInstanceData
		{
			public ChirrFriendTracker tracker;
		}

	}
}
