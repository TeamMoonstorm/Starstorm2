using EntityStates;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mimic
{
	public class MimicLeapLoop : BaseCharacterMain
	{

		public static float minimumDuration;
		public static float blastRadius;

		public static float blastProcCoefficient;
		public float blastDamageCoefficient;

		public float blastForce;
		public Vector3 blastBonusForce;
		public static float knockbackForce;

		public static string leapSoundString;

		public GameObject blastImpactEffectPrefab;
		public GameObject blastEffectPrefab;

		public static float airControl;

		public static float aimVelocity;
		public static float upwardVelocity;
		public static float forwardVelocity;

		public static float minimumY;
		public static float minYVelocityForAnim;
		public static float maxYVelocityForAnim;

		public static string soundLoopStartEvent;
		public static string soundLoopStopEvent;

		public static NetworkSoundEventDef landingSound;

		private float previousAirControl;
		protected bool isCritAuthority;
		private bool detonateNextFrame;

		private bool endedSuccessfully = false;

		public override void OnEnter()
		{
			base.OnEnter();
			PlayCrossfade("FullBody, Override", "LeapLoop", 0.05f);

			previousAirControl = characterMotor.airControl;
			characterMotor.airControl = airControl;

			Vector3 direction = GetAimRay().direction;
			if (isAuthority)
			{
				characterBody.isSprinting = true;
				direction.y = Mathf.Max(direction.y, minimumY);
				Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
				Vector3 b = Vector3.up * upwardVelocity;
				Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
				characterMotor.Motor.ForceUnground(0.1f);
				characterMotor.velocity = a + b + b2;
				isCritAuthority = RollCrit();
			}
			if (NetworkServer.active)
			{
				characterBody.AddTimedBuff(JunkContent.Buffs.IgnoreFallDamage, 0.25f, 1);
			}
			GetModelTransform().GetComponent<AimAnimator>().enabled = true;

			Util.PlaySound(leapSoundString, gameObject);
			characterDirection.moveVector = direction;

			if (isAuthority)
			{
				characterMotor.onMovementHit += OnMovementHit;
			}

			Util.PlaySound(soundLoopStartEvent, gameObject);
		}

		private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
		{
			detonateNextFrame = true;
		}

		public override void UpdateAnimationParameters()
		{
			base.UpdateAnimationParameters();
			float value = Mathf.Clamp01(Util.Remap(estimatedVelocity.y, minYVelocityForAnim, maxYVelocityForAnim, 0f, 1f)) * 0.97f;
			//modelAnimator.SetFloat("LeapCycle", value, 0.1f, Time.deltaTime);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (isAuthority && characterMotor)
			{
				characterMotor.moveDirection = inputBank.moveVector;
				if (fixedAge >= minimumDuration && (detonateNextFrame || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
				{
					DoImpactAuthority();
					endedSuccessfully = true;
					outer.SetNextState(new MimicLeapExit());
				}
			}
			if (NetworkServer.active)
			{
				characterBody.AddTimedBuff(JunkContent.Buffs.IgnoreFallDamage, 0.25f, 1);
			}
		}

		protected void DoImpactAuthority()
		{
			if (landingSound)
			{
				EffectManager.SimpleSoundEffect(landingSound.index, characterBody.footPosition, true);
			}
			DetonateAuthority();
		}

		protected BlastAttack.Result DetonateAuthority()
		{
			Vector3 footPosition = characterBody.footPosition;
			EffectManager.SpawnEffect(blastEffectPrefab, new EffectData
			{
				origin = footPosition,
				scale = blastRadius
			}, true);
				
			return new BlastAttack
			{
				attacker = gameObject,
				baseDamage = damageStat * blastDamageCoefficient,
				baseForce = blastForce,
				bonusForce = blastBonusForce,
				crit = isCritAuthority,
				damageType = DamageType.Generic,
				falloffModel = BlastAttack.FalloffModel.None,
				procCoefficient = blastProcCoefficient,
				radius = blastRadius,
				position = footPosition,
				attackerFiltering = AttackerFiltering.NeverHitSelf,
				impactEffect = EffectCatalog.FindEffectIndexFromPrefab(blastImpactEffectPrefab),
				teamIndex = teamComponent.teamIndex
			}.Fire();
		}

		public override void OnExit()
		{
			Util.PlaySound(soundLoopStopEvent, gameObject);
			if (isAuthority)
			{
				characterMotor.onMovementHit -= OnMovementHit;
			}
			characterMotor.airControl = previousAirControl;
			base.OnExit();

			if (!endedSuccessfully)
			{
				PlayAnimation("FullBody, Override", "BufferEmpty");
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
		
	}
}
