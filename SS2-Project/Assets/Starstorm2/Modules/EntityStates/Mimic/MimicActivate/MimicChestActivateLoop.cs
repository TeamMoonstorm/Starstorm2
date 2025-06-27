using R2API;
using RoR2;
using SS2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.Mimic
{
    public class MimicChestActivateLoop : BaseCharacterMain
	{
		public static float baseDuration;
		private float duration;
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
		private bool stopDoingThat = false;

		public override void OnEnter()
		{
			duration = baseDuration / attackSpeedStat;
			base.OnEnter();
			PlayCrossfade("FullBody, Override", "ActivateLoop", "Activate.playbackRate", duration, 0.05f);

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
				characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, duration);
			}
			GetModelTransform().GetComponent<AimAnimator>().enabled = true;

			Util.PlaySound(leapSoundString, gameObject);
			characterDirection.moveVector = direction;

			//if (isAuthority)
			//{
			//	characterMotor.onMovementHit += OnMovementHit;
			//}

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

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (isAuthority && characterMotor)
			{
				if (fixedAge >= duration && isAuthority)
				{
					//DetonateAuthority();
					endedSuccessfully = true;
					SS2Log.Warning("FixedUpdate Fire");
					outer.SetNextState(new MimicChestActivateExit());
				}
			}
			if (NetworkServer.active)
			{
				characterBody.AddTimedBuff(JunkContent.Buffs.IgnoreFallDamage, 0.25f, 1);
			}
		}

		protected BlastAttack.Result DetonateAuthority()
		{
			SS2Log.Warning("Detonate Authority");
			SS2Log.Warning("attacker : " + gameObject);
			SS2Log.Warning("damageStat : " + damageStat);
			SS2Log.Warning("blastDamageCoefficient : " + damageCoeff);
			SS2Log.Warning("blastForce : " + blastForce);
			SS2Log.Warning("blastBonusForce : " + blastBonusForce);
			SS2Log.Warning("isCritAuthority : " + isCritAuthority);
			SS2Log.Warning("blastProcCoefficient : " + blastProcCoefficient);
			SS2Log.Warning("blastRadius : " + blastRadius);
			SS2Log.Warning("characterBody.corePosition : " + characterBody.corePosition + " |" + transform.position);
			SS2Log.Warning("teamComponent.teamIndex : " + teamComponent.teamIndex);
			SS2Log.Warning("attacker : " + gameObject);


			var attack =  new BlastAttack
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
				teamIndex = teamComponent.teamIndex,
				
			};

			DamageAPI.AddModdedDamageType(attack, SS2.Monsters.Mimic.StealItemDamageType);

			var explosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/CryoCanisterExplosionSecondary.prefab").WaitForCompletion();
			EffectData effectData = new EffectData
			{
				origin = characterBody.corePosition
			};
			effectData.SetNetworkedObjectReference(this.gameObject);
			EffectManager.SpawnEffect(explosion, effectData, transmit: true);


			return attack.Fire(); 
		}
		
		public override void OnExit()
		{

			SS2Log.Info("MimicChestActivateLoop EXIT");
			Util.PlaySound(soundLoopStopEvent, gameObject);
			//if (isAuthority)
			//{
			//	characterMotor.onMovementHit -= OnMovementHit;
			//}
			characterMotor.airControl = previousAirControl;
			base.OnExit();

			if (!endedSuccessfully)
			{
				PlayAnimation("FullBody, Override", "BufferEmpty");
			}
            else
            {
				DetonateAuthority();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
