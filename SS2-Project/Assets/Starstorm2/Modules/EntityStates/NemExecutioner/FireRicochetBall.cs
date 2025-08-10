using System;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using RoR2.Orbs;
using R2API;
namespace EntityStates.NemExecutioner
{
	public class FireRicochetBall : BaseSkillState
	{
		public static GameObject projectilePrefab;
		public string soundString;
		public GameObject muzzleEffectPrefab = null;
		public string muzzleName;
		private static float bloom = 1f;
		private static float recoilAmplitude = 1f;
		private static float fireTime = 0.1f;
		private static float baseDuration = 0.4f;
		private static float damageCoefficient = 1.25f;
		private static float force = 100f;
		private static string fireSoundString = "Play_mage_m1_shoot";

		private bool hasFired;
		public float duration;

		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / attackSpeedStat;
			StartAimMode();
			base.PlayAnimation("Gesture, Override", "FirePrimary", "Primary.playbackRate", this.duration);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!this.hasFired && base.fixedAge >= this.duration * fireTime)
			{
				this.hasFired = true;

				this.Fire();
			}

			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		private void Fire()
		{
			Util.PlaySound(fireSoundString, base.gameObject);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, this.muzzleName, true);
			AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
			base.characterBody.AddSpreadBloom(bloom);

			Ray aimRay = GetAimRay();
			Vector3 direction = aimRay.direction;
			if (base.isAuthority)
			{
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(direction);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * damageCoefficient;
				fireProjectileInfo.force = force;
				fireProjectileInfo.crit = RollCrit();
				DamageTypeCombo damageType = DamageType.Generic;
				damageType.damageSource = DamageSource.Primary;
				fireProjectileInfo.damageTypeOverride = damageType;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}
