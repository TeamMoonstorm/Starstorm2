
using RoR2;
using Moonstorm.Starstorm2.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace EntityStates.Pyro
{
    public class Scorch : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
			this.firedShot = false;
            this.heatController = base.gameObject.GetComponent<PyroHeatComponent>();
			Util.PlaySound(Scorch.startAttackSoundString, base.gameObject); //TODO: USE LOOPING SOUND

			if (!Scorch.flamethrowerEffectPrefab)
            {
				Scorch.flamethrowerEffectPrefab = (Instantiate(typeof(EntityStates.Mage.Weapon.Flamethrower)) as EntityStates.Mage.Weapon.Flamethrower).flamethrowerEffectPrefab;
			}

			Transform modelTransform = base.GetModelTransform();
			if (modelTransform)
			{
				this.childLocator = modelTransform.GetComponent<ChildLocator>();
				this.muzzleTransform = this.childLocator.FindChild("MuzzleRight");
			}

			if (this.childLocator)
			{
				Transform transform2 = this.childLocator.FindChild("MuzzleRight");
				if (transform2)
				{
					this.flamethrowerTransform = UnityEngine.Object.Instantiate<GameObject>(Scorch.flamethrowerEffectPrefab, transform2).transform;
				}
				if (this.flamethrowerTransform)
				{
					this.flamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration = 2f;
				}
			}

			this.shotCounter = 0;
			this.flamethrowerStopwatch = 0f;
			this.selfForceStopwatch = 0f;
			this.tickDuration = Scorch.baseTickDuration / this.attackSpeedStat;

			this.flamethrowerEffectResetStopwatch = 0f;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.flamethrowerStopwatch += Time.fixedDeltaTime;
			this.flamethrowerEffectResetStopwatch += Time.fixedDeltaTime;
			this.selfForceStopwatch += Time.fixedDeltaTime;
			this.tickDuration = Scorch.baseTickDuration / this.attackSpeedStat;

			if (base.characterBody)
            {
				base.characterBody.isSprinting = false;
			}

			if (base.isAuthority && this.selfForceStopwatch > Scorch.baseTickDuration)
            {
				this.selfForceStopwatch -= Scorch.baseTickDuration;
				if (base.characterMotor && !base.characterMotor.isGrounded)
				{
					this.selfForceDirection = base.GetAimRay().direction;
					this.selfForceDirection.x *= 0.25f;
					this.selfForceDirection.y = Mathf.Min(0f, this.selfForceDirection.y);
					this.selfForceDirection.z *= 0.25f;
					base.characterMotor.ApplyForce(this.selfForceDirection * -Scorch.selfForce, false, false);
				}
			}

			if (this.flamethrowerEffectResetStopwatch > Scorch.flamethrowerEffectResetTimer)	//hacky stuff to get arti's flamethrower effect to loop
            {
				this.flamethrowerEffectResetStopwatch = 0f;
				EntityState.Destroy(this.flamethrowerTransform.gameObject);
				if (this.childLocator)
				{
					Transform transform2 = this.childLocator.FindChild("MuzzleRight");
					if (transform2)
					{
						this.flamethrowerTransform = UnityEngine.Object.Instantiate<GameObject>(Scorch.flamethrowerEffectPrefab, transform2).transform;
					}
					if (this.flamethrowerTransform)
					{
						this.flamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration = 2f;
					}
				}
			}

			if (this.flamethrowerStopwatch > this.tickDuration)
			{
				while (this.flamethrowerStopwatch - this.tickDuration > 0f)
                {
					this.flamethrowerStopwatch -= this.tickDuration;
				}
				this.ShootFlame();
			}
			this.UpdateFlamethrowerEffect();
			if ((!base.inputBank || !base.inputBank.skill1.down) && this.firedShot && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		public override void OnExit()
        {
			Util.PlaySound(Scorch.endAttackSoundString, base.gameObject);
			if (this.flamethrowerTransform)
			{
				EntityState.Destroy(this.flamethrowerTransform.gameObject);
			}
			base.OnExit();
		}

        private void ShootFlame()
        {
			this.firedShot = true;
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(2f);
			}
			Ray aimRay = base.GetAimRay();
			if (base.isAuthority)
			{
				this.shotCounter = (this.shotCounter + 1) % burnFrequency;
				new BulletAttack
				{
					owner = base.gameObject,
					weapon = base.gameObject,
					origin = aimRay.origin,
					aimVector = aimRay.direction,
					minSpread = 0f,
					damage = Scorch.damageCoefficient * this.damageStat,
					force = Scorch.force,
					muzzleName = "",
					hitEffectPrefab = Scorch.impactEffectPrefab,
					isCrit = base.RollCrit(),
					radius = Scorch.radius,
					falloffModel = BulletAttack.FalloffModel.None,
					stopperMask = LayerIndex.world.mask,
					procCoefficient = Scorch.procCoefficient,
					maxDistance = Scorch.maxDistance,
					smartCollision = true,
					damageType = (heatController.IsHighHeat() && this.shotCounter == burnFrequency - 1 ? DamageType.IgniteOnHit : DamageType.Generic)
				}.Fire();
				heatController.AddHeat(Scorch.heatPerTick);
				base.characterBody.AddSpreadBloom(0.4f);
			}
		}

		private void UpdateFlamethrowerEffect()
		{
			if (this.flamethrowerTransform)
			{
				this.flamethrowerTransform.forward = base.GetAimRay().direction;
			}
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
			return InterruptPriority.Skill;
        }

        public static string startAttackSoundString = "Play_mage_R_start";
		public static string endAttackSoundString = "Play_mage_R_end";
		public static GameObject flamethrowerEffectPrefab = null;
		public static GameObject impactEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/missileexplosionvfx");

		public static float selfForce = 420f;

		public static float maxDistance = 21f;
		public static float procCoefficient = 0.75f;
		public static float radius = 2.4f;
		public static float force = 0f;
		public static float damageCoefficient = 0.6f;
        public static float baseTickDuration = 0.16f;
        public static float heatPerTick = 0.025f;
        public static int burnFrequency = 5;    //When at high heat, apply ignite every X shots. Higher = less frequent, 1 = every shot.

        private PyroHeatComponent heatController;
		private Transform flamethrowerTransform;
		private Transform muzzleTransform;
		private ChildLocator childLocator;
		private float flamethrowerStopwatch;
		private int shotCounter;
		private float tickDuration;
		private bool firedShot;
		private Vector3 selfForceDirection;
		private float selfForceStopwatch;

		private static float flamethrowerEffectResetTimer = 1.8f;
		private float flamethrowerEffectResetStopwatch;
	}
}