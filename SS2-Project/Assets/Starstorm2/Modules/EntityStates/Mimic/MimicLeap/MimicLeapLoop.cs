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

			var lThruster = FindModelChild("ThrusterL");
			var rThruster = FindModelChild("ThrusterR");

			string effectName = "RoR2/Base/Commando/CommandoDashJets.prefab";
			var effect = Addressables.LoadAssetAsync<GameObject>(effectName).WaitForCompletion();

			if (lThruster)
            {
				UnityEngine.Object.Instantiate<GameObject>(effect, lThruster);
			}

            if (rThruster)
            {
				UnityEngine.Object.Instantiate<GameObject>(effect, rThruster);
			}


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
					SS2Log.Warning("FixedUpdate Fire");
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
			SS2Log.Warning("DoImpactAuthority");
			if (landingSound)
			{
				EffectManager.SimpleSoundEffect(landingSound.index, characterBody.footPosition, true);
			}
			DetonateAuthority();
		}

		protected BlastAttack.Result DetonateAuthority()
		{
			//Vector3 footPosition = characterBody.corePosition;
			//EffectManager.SpawnEffect(blastEffectPrefab, new EffectData
			//{
			//	origin = footPosition,
			//	scale = blastRadius
			//}, true);
			SS2Log.Warning("Detonate Authority");
			SS2Log.Warning("attacker : " + gameObject);
			SS2Log.Warning("damageStat : " + damageStat);
			SS2Log.Warning("damageCoeff : " + damageCoeff);
			SS2Log.Warning("blastForce : " + blastForce);
			SS2Log.Warning("blastBonusForce : " + blastBonusForce);
			SS2Log.Warning("isCritAuthority : " + isCritAuthority);
			SS2Log.Warning("blastProcCoefficient : " + blastProcCoefficient);
			SS2Log.Warning("blastRadius : " + blastRadius);
			SS2Log.Warning("characterBody.corePosition : " + characterBody.corePosition + " |" + transform.position);
			SS2Log.Warning("teamComponent.teamIndex : " + teamComponent.teamIndex);
			SS2Log.Warning("attacker : " + gameObject);

			var explosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/CryoCanisterExplosionSecondary.prefab").WaitForCompletion();
			EffectData effectData = new EffectData
			{
				origin = characterBody.corePosition
			};
			effectData.SetNetworkedObjectReference(this.gameObject);
			EffectManager.SpawnEffect(explosion, effectData, transmit: true);

			return new BlastAttack
			{
				attacker = gameObject,
				baseDamage = damageStat * damageCoeff,
				baseForce = blastForce,
				bonusForce = blastBonusForce,
				crit = isCritAuthority,
				damageType = DamageType.Generic,
				falloffModel = BlastAttack.FalloffModel.None,
				procCoefficient = blastProcCoefficient,
				radius = blastRadius,
				position = characterBody.corePosition,
				attackerFiltering = AttackerFiltering.NeverHitSelf,
				//impactEffect = EffectCatalog.FindEffectIndexFromPrefab(blastImpactEffectPrefab),
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
