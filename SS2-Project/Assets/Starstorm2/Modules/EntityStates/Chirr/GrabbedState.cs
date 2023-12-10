﻿using System;
using EntityStates;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2;
namespace EntityStates
{
	public class GrabbedState : BaseState
	{
		public float duration = Mathf.Infinity;

		public override void OnEnter()
		{
			base.OnEnter();
			Animator modelAnimator = base.GetModelAnimator();

			SS2Log.Warning("GRABBED");
			if (modelAnimator)
			{
				int layerIndex = modelAnimator.GetLayerIndex("Body");
				modelAnimator.enabled = false;
				modelAnimator.CrossFadeInFixedTime((UnityEngine.Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2", 0.1f);
				modelAnimator.Update(0f);
			}
		}
		public override void OnExit()
		{
			Animator modelAnimator = base.GetModelAnimator();
			if (modelAnimator)
			{
				modelAnimator.enabled = true;
			}
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if(base.isAuthority && base.fixedAge >= this.duration)
				this.outer.SetNextStateToMain();		
		}


		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Vehicle;
		}


	}
}