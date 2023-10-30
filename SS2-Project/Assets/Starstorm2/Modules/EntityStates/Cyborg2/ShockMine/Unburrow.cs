using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using Moonstorm.Starstorm2.Components;
using UnityEngine;
namespace EntityStates.Cyborg2.ShockMine
{
	public class Unburrow : BaseShockMineState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = Unburrow.baseDuration;

			Vector3 upVelocity = Vector3.up * hopVelocity;
			base.rigidbody.velocity = upVelocity;// base.transform.rotation * upVelocity;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority)
			{
				if(base.fixedAge >= this.duration)
                {
					this.outer.SetNextState(new ShockNearby());
                }
			}
		}

		protected override bool shouldStick => false;

		public static float baseDuration = .67f;

		public static float hopVelocity = 20f;
		private float duration;
	}
}
