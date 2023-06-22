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
	[CreateAssetMenu(menuName = "Starstorm2/SkillDef/NemMercTrackingSkillDef")]

	public class NemMercTrackingSkillDef : SkillDef
	{
		public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
		{
			return new NemMercTrackingSkillDef.InstanceData
			{
				tracker = skillSlot.GetComponent<NemMercTracker>()
			};

			//skillSlot.characterBody.onki
		}

		public static bool HasTarget([NotNull] GenericSkill skillSlot)
		{
			NemMercTracker tracker = ((NemMercTrackingSkillDef.InstanceData)skillSlot.skillInstanceData).tracker;
			return (tracker != null) ? tracker.GetTrackingTarget() : null;
		}

		public override bool CanExecute([NotNull] GenericSkill skillSlot)
		{
			return NemMercTrackingSkillDef.HasTarget(skillSlot) && base.CanExecute(skillSlot);
		}
		public override bool IsReady([NotNull] GenericSkill skillSlot)
		{
			return base.IsReady(skillSlot) && NemMercTrackingSkillDef.HasTarget(skillSlot);
		}

		public class InstanceData : SkillDef.BaseSkillInstanceData
		{
			public NemMercTracker tracker;
		}

	}
}
