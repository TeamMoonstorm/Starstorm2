using RoR2;
using SS2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mimic.Weapon
{
	public class MimicMinigunState : BaseState
	{
		public static string muzzleNameLeft;
		public static string muzzleNameRight;

		protected static readonly BuffDef slowBuff = SS2Content.Buffs.bdHiddenSlow20;

		protected Transform muzzleTransformLeft;
		protected Transform muzzleTransformRight;

		public GameObject fireVFXInstanceLeft;
		public GameObject fireVFXInstanceRight;

		public override void OnEnter()
		{
			base.OnEnter();
			muzzleTransformLeft = FindModelChild(muzzleNameLeft);
			muzzleTransformRight = FindModelChild(muzzleNameRight);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			StartAimMode(2f, false);
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		protected ref InputBankTest.ButtonState skillButtonState
		{
			get
			{
				return ref inputBank.skill1;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		public override void ModifyNextState(EntityState nextState)
		{
			base.ModifyNextState(nextState);
			if(nextState is MimicMinigunState minigunState)
            {
                if (this.fireVFXInstanceLeft)
                {
					minigunState.fireVFXInstanceLeft = this.fireVFXInstanceLeft;
				}

				if (this.fireVFXInstanceRight)
				{
					minigunState.fireVFXInstanceRight = this.fireVFXInstanceRight;
				}

			}

		}
	}
}