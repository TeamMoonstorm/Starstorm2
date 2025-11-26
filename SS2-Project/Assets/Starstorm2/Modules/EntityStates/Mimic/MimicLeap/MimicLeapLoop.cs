using EntityStates;
using RoR2;
using SS2;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.Mimic
{
	public class MimicLeapLoop : BaseCharacterMain
	{

		public static float minimumDuration;
		public static float blastRadius;

		public static float blastProcCoefficient;
		public static float damageCoeff;

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
				Vector3 a = direction.normalized * aimVelocity * moveSpeedStat/2;
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

			var lThruster = FindModelChild("ThrusterL");
			var rThruster = FindModelChild("ThrusterR");

			if (lThruster)
            {
				UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.jetVFX, lThruster);
			}

            if (rThruster)
            {
				UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.jetVFX, rThruster);
			}
		}

		private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
		{
			detonateNextFrame = true;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (characterMotor)
			{
				if (fixedAge >= minimumDuration && (detonateNextFrame || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
				{
					endedSuccessfully = true;
                    if (isAuthority)
                    {
						characterMotor.moveDirection = inputBank.moveVector;
						DoImpactAuthority();
						outer.SetNextState(new MimicLeapExit());
					}
				}
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
			EffectData effectData = new EffectData
			{
				origin = characterBody.corePosition
			};
			effectData.SetNetworkedObjectReference(this.gameObject);
			EffectManager.SpawnEffect(SS2.Monsters.Mimic.leapLandVFX, effectData, transmit: true);

			DamageTypeCombo damageTypeCombo = DamageType.Generic;
			damageTypeCombo.damageSource = DamageSource.Secondary;
			return new BlastAttack
			{
				attacker = gameObject,
				baseDamage = damageStat * damageCoeff,
				baseForce = blastForce,
				bonusForce = blastBonusForce,
				crit = isCritAuthority,
				damageType = damageTypeCombo,
				falloffModel = BlastAttack.FalloffModel.None,
				procCoefficient = blastProcCoefficient,
				radius = blastRadius,
				position = characterBody.corePosition,
				attackerFiltering = AttackerFiltering.NeverHitSelf,
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
