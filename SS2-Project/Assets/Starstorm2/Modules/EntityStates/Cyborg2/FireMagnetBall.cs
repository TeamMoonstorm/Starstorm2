using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
namespace EntityStates.Cyborg2
{
	public class FireMagnetBall : BaseSkillState
	{
		public static GameObject projectilePrefab;
		public static string soundString = "Play_captain_m2_tazer_shoot";
		public static GameObject muzzleEffectPrefab;

		public static float bloom = 0.5f;
		public static float recoilAmplitude = 7f;
		public static float baseDuration = 1.5f;
		public static float earlyExitTime = 0.5f;
		public static float damageCoefficient = 6f;
		public static float force = 500f;
		public static float chargeTime = 0.33f;

		[NonSerialized]
		public static float selfKnockbackForce = 4400f;

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
			if(base.fixedAge >= this.duration * chargeTime && !this.hasFired)
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
				if (base.characterMotor && !base.characterMotor.isGrounded)
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
