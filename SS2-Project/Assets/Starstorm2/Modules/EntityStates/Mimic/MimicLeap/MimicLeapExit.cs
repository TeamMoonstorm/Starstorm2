using EntityStates;
using EntityStates.Mimic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Mimic
{
	public class MimicLeapExit : BaseState
	{
		public static float baseDuration;
		private float duration;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;
			PlayCrossfade("FullBody, Override", "LeapExit", "Leap.playbackRate", duration, 0.05f);
			//duration += .02f;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration && isAuthority)
			{
				characterBody.isSprinting = false;
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			//PlayAnimation("FullBody, Override", "BufferEmpty");
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
