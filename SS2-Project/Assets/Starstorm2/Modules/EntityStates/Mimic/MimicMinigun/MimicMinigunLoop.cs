using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mimic.Weapon
{
    public class MimicMinigunLoop : MimicMinigunState
    {
		public static float baseFireInterval;
		public static int baseBulletCount;

		public static float baseDamagePerSecondCoefficient;
		public static float baseProcCoefficientPerSecond;

		public static float bulletMinSpread;
		public static float bulletMaxSpread;

		public static GameObject muzzleVfxPrefab;
		public static GameObject bulletTracerEffectPrefab;
		public static GameObject bulletHitEffectPrefab;

		public static bool bulletHitEffectNormal;
		public static float bulletMaxDistance;

		public static string fireSound;
		public static string startSound;
		public static string endSound;

		private float fireTimer;
		private float baseFireRate;
		private float baseBulletsPerSecond;

		private Run.FixedTimeStamp critEndTime;
		private Run.FixedTimeStamp lastCritCheck;

		public static string mecanimPeramater;

		private bool endedSuccessfully = false;

		public override void OnEnter()
		{
			base.OnEnter();

			baseFireRate = 1f / baseFireInterval;
			baseBulletsPerSecond = (float)baseBulletCount * this.baseFireRate;

			critEndTime = Run.FixedTimeStamp.negativeInfinity;
			lastCritCheck = Run.FixedTimeStamp.negativeInfinity;

			PlayCrossfade("Gesture, Override", "MinigunLoop", 0.05f);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			this.fireTimer -= base.GetDeltaTime();
			if (this.fireTimer <= 0f)
			{
				float num = baseFireInterval / this.attackSpeedStat;
				this.fireTimer += num;
				this.OnFireShared();
			}
			if (base.isAuthority && !base.skillButtonState.down)
			{
				var exit = new MimicMinigunExit { fireVFXInstanceLeft = this.fireVFXInstanceLeft, fireVFXInstanceRight = this.fireVFXInstanceRight };
				this.outer.SetNextState(exit);
				return;
			}
		}

		private void UpdateCrits()
		{
			if (this.lastCritCheck.timeSince >= 1f)
			{
				this.lastCritCheck = Run.FixedTimeStamp.now;
				if (base.RollCrit())
				{
					this.critEndTime = Run.FixedTimeStamp.now + 2f;
				}
			}
		}


		public override void OnExit()
		{
			base.OnExit();
			Util.PlaySound(endSound, base.gameObject);

			if (!endedSuccessfully)
			{
				PlayAnimation("Gesture, Override", "BufferEmpty");
				if (fireVFXInstanceLeft)
				{
					EntityState.Destroy(fireVFXInstanceLeft);
				}

				if (fireVFXInstanceRight)
				{
					EntityState.Destroy(fireVFXInstanceRight);
				}
			}
		}


		private void OnFireShared()
		{
			Util.PlaySound("Play_commando_M1", gameObject);
			if (base.isAuthority)
			{
				this.OnFireAuthority();
			}
		}


		private void OnFireAuthority()
		{
			this.UpdateCrits();
			bool isCrit = !critEndTime.hasPassed;
			float damage = baseDamagePerSecondCoefficient / baseBulletsPerSecond * damageStat;
			float procCoefficient = baseProcCoefficientPerSecond / baseBulletsPerSecond;
			Ray aimRay = base.GetAimRay();
			
			new BulletAttack
			{
				bulletCount = (uint)baseBulletCount,
				aimVector = aimRay.direction,
				origin = aimRay.origin,
				damage = damage,
				damageColorIndex = DamageColorIndex.Default,
				damageType = DamageType.Generic,
				falloffModel = BulletAttack.FalloffModel.None,
				maxDistance = bulletMaxDistance,
				force = 0,
				hitMask = LayerIndex.CommonMasks.bullet,
				minSpread = bulletMinSpread,
				maxSpread = bulletMaxSpread,
				isCrit = isCrit,
				owner = base.gameObject,
				muzzleName = muzzleNameLeft,
				smartCollision = false,
				procChainMask = default(ProcChainMask),
				procCoefficient = procCoefficient,
				radius = 0f,
				sniper = false,
				stopperMask = LayerIndex.CommonMasks.bullet,
				weapon = null,
				tracerEffectPrefab = bulletTracerEffectPrefab,
				spreadPitchScale = 1f,
				spreadYawScale = 1f,
				queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
				hitEffectPrefab = bulletHitEffectPrefab,
				HitEffectNormal = bulletHitEffectNormal
			}.Fire();

			new BulletAttack
			{
				bulletCount = (uint)baseBulletCount,
				aimVector = aimRay.direction,
				origin = aimRay.origin,
				damage = damage,
				damageColorIndex = DamageColorIndex.Default,
				damageType = DamageType.Generic,
				falloffModel = BulletAttack.FalloffModel.None,
				maxDistance = bulletMaxDistance,
				force = 0,
				hitMask = LayerIndex.CommonMasks.bullet,
				minSpread = bulletMinSpread,
				maxSpread = bulletMaxSpread,
				isCrit = isCrit,
				owner = base.gameObject,
				muzzleName = muzzleNameRight,
				smartCollision = false,
				procChainMask = default(ProcChainMask),
				procCoefficient = procCoefficient,
				radius = 0f,
				sniper = false,
				stopperMask = LayerIndex.CommonMasks.bullet,
				weapon = null,
				tracerEffectPrefab = bulletTracerEffectPrefab,
				spreadPitchScale = 1f,
				spreadYawScale = 1f,
				queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
				hitEffectPrefab = bulletHitEffectPrefab,
				HitEffectNormal = bulletHitEffectNormal
			}.Fire();
		}
    }
}
