using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EntityStates.Chirr
{
	public class SpitBomb : BaseSkillState
	{

		public static GameObject projectilePrefab;
		public string soundString;
		public GameObject muzzleEffectPrefab = null;
		public string muzzleName;
		public static float bloom = 1f;
		public static float recoilAmplitude = 1f;
		public static float fireTime = 0.5f;
		public static float baseDuration = 0.5f;
		public static float damageCoefficient = 2f;
		public static float force = 100f;

		public static float selfAwayForce = 11f;
		public static float selfUpForce = 11f;
		private bool hasFired;
		public float duration;

		[NonSerialized]
		private static bool additivetest = true;
		[NonSerialized]
		private static bool fullbodytest = true;
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / attackSpeedStat;
			StartAimMode();
			string layerName = fullbodytest ? "FullBody, " : "Gesture, ";
			layerName += additivetest ? "Additive" : "Override";
			base.PlayAnimation(layerName, "FireSecondary", "Secondary.playbackRate", this.duration);
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
			Util.PlaySound("ChirrFireSpitBomb", base.gameObject);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, this.muzzleName, true);
			AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
			base.characterBody.AddSpreadBloom(bloom);

			Ray aimRay = GetAimRay();
			Vector3 direction = aimRay.direction;
			if (base.isAuthority)
			{
				Vector3 awayForce = -1f * direction * selfAwayForce;
				awayForce += Vector3.up * selfUpForce;
				if(base.characterMotor && !base.isGrounded)
                {
					// the skilldef has cancel sprint=false, this apparently is stopping sprint anyways
					float upVelocity = Mathf.Max(base.characterMotor.velocity.y, awayForce.y);
					base.characterMotor.velocity = new Vector3(characterMotor.velocity.x + awayForce.x, upVelocity, characterMotor.velocity.z + awayForce.z);
					if(EntityStateMachine.FindByCustomName(base.gameObject, "Wings")?.state.GetType() != typeof(Idle)) // jank to keep sprinting while hovering
                    {
						base.characterBody.isSprinting = true;
                    }
				}
					

				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = aimRay.origin;
				fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(direction);
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * damageCoefficient;
				fireProjectileInfo.force = force;
				fireProjectileInfo.crit = RollCrit();
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
