using EntityStates;
using EntityStates.Mimic;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using RoR2.CharacterAI;
using SS2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MimicEquip
{
	public class MimicEquipLeapEnter : BaseState
	{
		public static float baseDuration;
		private float duration;
		private bool endedSuccessfully = false;
		private BaseAI ai;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;

			PlayCrossfade("FullBody, Override", "LeapEnter", "Leap.playbackRate", duration, 0.05f);

			var animator = GetModelAnimator();
			animator.SetBool("isGrounded", false);

			characterMotor.walkSpeedPenaltyCoefficient += .6f;
			characterBody.skillLocator.secondary.AddOneStock(); // dont rteally have a better spot just need it to be added <3 .,
		}
		
		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (fixedAge >= duration)
			{
				characterMotor.walkSpeedPenaltyCoefficient -= .6f;
				endedSuccessfully = true;
				if (isAuthority)
                {
					outer.SetNextState(new MimicEquipLeapLoop());
				}
			}
		}

		public override void OnExit()
		{
            if (!endedSuccessfully)
            {
				PlayAnimation("FullBody, Override", "BufferEmpty");
            }
            base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
