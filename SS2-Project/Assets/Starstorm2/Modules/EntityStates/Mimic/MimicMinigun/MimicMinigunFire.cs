using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mimic.Weapon
{
    public class MimicMinigunLoop : MimicMinigunState
    {
        //public static float damageCoefficient;
        //public static float fireRate;
        //public static float recoil;
        //public static float range;
        //public static GameObject tracerEffectPrefab;

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

        private float duration;
        private bool hasFired;
		//private Transform muzzle;
		private Animator animator;

		private bool endedSuccessfully = false;

		public override void OnEnter()
		{
			base.OnEnter();
			//if (this.muzzleTransform && MinigunFire.muzzleVfxPrefab)
			//{
			//	this.muzzleVfxTransform = UnityEngine.Object.Instantiate<GameObject>(MinigunFire.muzzleVfxPrefab, this.muzzleTransform).transform;
			//}
			//if (NetworkServer.active && base.characterBody)
			//{
				//characterBody.AddBuff(slowBuff);
			//}

			baseFireRate = 1f / baseFireInterval;
			baseBulletsPerSecond = (float)baseBulletCount * this.baseFireRate;

			critEndTime = Run.FixedTimeStamp.negativeInfinity;
			lastCritCheck = Run.FixedTimeStamp.negativeInfinity;
			//Util.PlaySound(MinigunFire.startSound, base.gameObject);


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
				this.outer.SetNextState(new MimicMinigunExit());
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
			Util.PlaySound(endSound, base.gameObject);
			//if (this.muzzleVfxTransform)
			//{
			//	EntityState.Destroy(this.muzzleVfxTransform.gameObject);
			//	this.muzzleVfxTransform = null;
			//}
			//base.PlayCrossfade("Gesture, Additive", "BufferEmpty", 0.2f);
			//if (NetworkServer.active && base.characterBody)
			//{
			//	characterBody.RemoveBuff(slowBuff);
			//}

			base.OnExit();

			if (!endedSuccessfully)
			{
				PlayAnimation("Gesture, Override", "BufferEmpty");
			}
		}


		private void OnFireShared()
		{
			//Util.PlaySound(fireSound, base.gameObject);
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
			//float force = baseForcePerSecond / baseBulletsPerSecond;
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
