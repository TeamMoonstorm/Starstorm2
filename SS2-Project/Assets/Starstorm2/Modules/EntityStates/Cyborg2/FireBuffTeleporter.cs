using System;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
namespace EntityStates.Cyborg2
{
	public class FireBuffTeleporter : BaseSkillState
	{
		public static GameObject projectilePrefab;
		public static string soundString = "Play_captain_m2_tazer_shoot";
		public static GameObject muzzleEffectPrefab;

		private static float bloom = 0.5f;
		private static float recoilAmplitude = 7f;
		private static float baseDuration = 0.6f;
		private static float earlyExitTime = 0.5f;
		private static float damageCoefficient = 6f;
		private static float force = 500f;
		private static float chargeTime = 0.33f;

		private static float selfKnockbackForce = 6000f;

		private float duration;
		private bool hasFired;
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / attackSpeedStat;
			StartAimMode();
			//anim
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration * chargeTime && !this.hasFired)
			{
				this.Fire();
			}
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}


		private void Fire()
		{
			this.hasFired = true;
			Util.PlaySound(soundString, base.gameObject);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, "CannonR", true);
			AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
			base.characterBody.AddSpreadBloom(bloom);
			Ray aimRay = GetAimRay();


			if (base.isAuthority)
			{
				if (base.characterMotor)
				{
					base.characterMotor.ApplyForce((aimRay.direction * -1f) * selfKnockbackForce);
				}

				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * damageCoefficient;
				fireProjectileInfo.force = force;
				fireProjectileInfo.crit = RollCrit();
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return (base.fixedAge >= this.duration * earlyExitTime) ? InterruptPriority.Any : InterruptPriority.Pain;
		}
	}
}
