using System;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Cyborg2
{
	public class FireLaser : GenericBulletBaseState
	{

		public static GameObject TRACERTEMP = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();
		public static float earlyExitTime = 0.33f;
		public static float selfKnockbackForce = 4400f;
		public override void OnEnter()
		{
			baseDuration = 1.5f;
			damageCoefficient = 10f;
			bulletRadius = 3f;
			force = 4000f;
			tracerEffectPrefab = TRACERTEMP;
			maxDistance = 1000f;
			muzzleName = "CannonR";
			spreadBloomValue = 5f;
			//muzzleFlashPrefab = Assets.muzzleFlashRailgun;

			base.OnEnter();
		}

		public override void FireBullet(Ray aimRay)
		{
			base.FireBullet(aimRay);

			if (base.isAuthority)
			{
				if (base.characterMotor && !base.characterMotor.isGrounded)
				{
					base.characterMotor.ApplyForce((aimRay.direction * -1f) * selfKnockbackForce);
				}
			}
		}

		public override void DoFireEffects()
		{
			base.DoFireEffects();
			Util.PlaySound("Play_railgunner_R_fire", base.gameObject);

		}

		public override void ModifyBullet(BulletAttack bulletAttack)
		{
			bulletAttack.stopperMask = 0;
			bulletAttack.tracerEffectPrefab = TRACERTEMP;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
		}

		public override void PlayFireAnimation()
		{
			//
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return base.fixedAge >= this.duration * earlyExitTime ? InterruptPriority.Any : InterruptPriority.PrioritySkill;
		}

	}
}