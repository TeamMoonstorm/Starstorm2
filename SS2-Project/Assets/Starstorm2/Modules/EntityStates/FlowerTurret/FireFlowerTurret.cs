using System;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using System.Collections.Generic;
using Moonstorm.Starstorm2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;
namespace EntityStates.FlowerTurret
{
	public class FireFlowerTurret : BaseFlowerTurretState
	{
		public static float baseDuration = 0.75f;
		public static float damageCoefficient = 1f;
		public static float force = 100f;
		public static float procCoefficient = 1f;
		public static string muzzleName = "Muzzle";
		public static GameObject muzzleFlashPrefab;
		public HurtBox targetHurtBox;
		private float duration;

		private bool hasFired;
		public float fireTime = 0.23f;
		public override void OnEnter()
		{
			base.OnEnter();		
			this.duration = baseDuration;
			if(this.body)
            {
				this.duration = baseDuration / this.body.attackSpeed;
            }	
			if (this.animator)
			{
				EntityState.PlayAnimationOnAnimator(this.animator, "Body", "FireGoo", "Primary.playbackRate", this.duration);

			}
		}

		private void Fire()
        {
			if (!muzzleTransform)
			{
				muzzleTransform = this.body.coreTransform;
			}
			if (NetworkServer.active)
			{
				if (this.targetHurtBox)
				{
					FlowerOrb flowerOrb = new FlowerOrb();
					flowerOrb.forceScalar = force;
					flowerOrb.damageValue = this.body.damage * damageCoefficient;
					flowerOrb.isCrit = Util.CheckRoll(this.body.crit, this.body.master);
					flowerOrb.teamIndex = this.body.teamComponent.teamIndex;
					flowerOrb.attacker = this.body.gameObject;
					flowerOrb.procCoefficient = procCoefficient;
					HurtBox hurtBox2 = this.targetHurtBox;
					if (hurtBox2)
					{
						EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, this.networkedBodyAttachment.gameObject, "Muzzle", true);
						flowerOrb.origin = muzzleTransform.position;
						flowerOrb.target = hurtBox2;
						OrbManager.instance.AddOrb(flowerOrb);
					}
				}
			}
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if(base.fixedAge > this.duration * fireTime && !this.hasFired)
            {
				this.Fire();
				this.hasFired = true;
			}
			if (base.fixedAge > this.duration)
			{
				this.outer.SetNextStateToMain();
			}
		}

		
		

		
	}
}
