using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EntityStates.NemMerc
{
	public class ThrowKnife : BaseSkillState
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

		public static float projectileHSpeed = 140f;

		private bool hasFired;
		public float duration;

		public static float autoAimRadius = 2.5f;
		public static float autoAimDistance = 50f;


		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = baseDuration / attackSpeedStat;
			StartAimMode();
			PlayAnimation("Gesture, Override", "ThrowKnife", "Secondary.playbackRate", duration * 2);
			if(!base.isGrounded)
				PlayAnimation("Body", "Jump");
			Util.PlaySound("Play_nemmerc_secondary_lunge", base.gameObject);

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

			if(!base.isGrounded)
				base.SmallHop(base.characterMotor, 1f);
		}

		private void Fire()
		{
			Util.PlaySound("Play_nemmerc_knife_throw", base.gameObject);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, this.muzzleName, true);
			AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
			base.characterBody.AddSpreadBloom(bloom);

			Ray aimRay = GetAimRay();
			Vector3 direction = aimRay.direction;

			float autoAimSpeed = -1f;
			if(Util.CharacterSpherecast(base.gameObject, aimRay, autoAimRadius, out RaycastHit hitInfo, autoAimDistance, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
            {
				Vector3 position = hitInfo.point;
				HurtBox hurtBox = hitInfo.collider.GetComponent<HurtBox>();
				if (hurtBox)
                {
					position = hurtBox.transform.position;
                }

				Vector3 between = position - aimRay.origin;
				Vector2 horizontal = new Vector2(between.x, between.z);
				float magnitude2 = horizontal.magnitude;
				float y = Trajectory.CalculateInitialYSpeed(magnitude2 / projectileHSpeed, between.y);
				Vector3 a = new Vector3(horizontal.normalized.x * projectileHSpeed, y, horizontal.normalized.y * projectileHSpeed);
				autoAimSpeed = a.magnitude;
				direction = a.normalized;
			}


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
				fireProjectileInfo.speedOverride = autoAimSpeed;
				ProjectileManager.instance.FireProjectile(fireProjectileInfo);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
