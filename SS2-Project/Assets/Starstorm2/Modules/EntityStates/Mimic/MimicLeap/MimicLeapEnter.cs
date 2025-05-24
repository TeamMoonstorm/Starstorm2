using EntityStates;
using EntityStates.Mimic;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Mimic
{
	public class MimicLeapEnter : BaseState
	{
		public static float baseDuration;
		private float duration;
		private bool endedSuccessfully = false;
		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;

			PlayCrossfade("FullBody, Override", "LeapEnter", "Leap.playbackRate", duration, 0.05f);

			characterMotor.walkSpeedPenaltyCoefficient += .6f;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (fixedAge >= duration && isAuthority)
			{
				characterMotor.walkSpeedPenaltyCoefficient -= .6f;
				endedSuccessfully = true;
				outer.SetNextState(new MimicLeapLoop());
			}
		}

		public override void OnExit()
		{
			base.OnExit();
            if (!endedSuccessfully)
            {
				PlayAnimation("FullBody, Override", "BufferEmpty");
            }
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
