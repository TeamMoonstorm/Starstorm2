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

		private bool endedSuccessfully = false;

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

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (isAuthority && characterMotor)
			{
				if (fixedAge >= duration && isAuthority)
				{
					endedSuccessfully = true;
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
				teamIndex = teamComponent.teamIndex,
			};

			DamageAPI.AddModdedDamageType(attack, SS2.Monsters.Mimic.StealItemDamageType);

			EffectData effectData = new EffectData
			{
				origin = characterBody.corePosition
			};
			effectData.SetNetworkedObjectReference(this.gameObject);
			EffectManager.SpawnEffect(SS2.Monsters.Mimic.leapLandVFX, effectData, transmit: true);

			return attack.Fire(); 
		}
		
		public override void OnExit()
		{
			base.OnExit();

			Util.PlaySound(soundLoopStopEvent, gameObject);
			characterMotor.airControl = previousAirControl;

			if (!endedSuccessfully)
			{
				PlayAnimation("FullBody, Override", "BufferEmpty");
			}
            else if(NetworkServer.active && isAuthority)
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
