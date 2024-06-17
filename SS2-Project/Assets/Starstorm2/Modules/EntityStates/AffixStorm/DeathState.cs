using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2;
using System.Collections.Generic;
using RoR2.Orbs;
namespace EntityStates.AffixStorm
{
	public class DeathState : BaseState
	{
		public static GameObject spawnEffectPrefab;

		private static float baseDuration = 0.3f;

		private static float searchRadius = 32f;
		private static float damageCoefficient = 3f;
		private static float percentHealthDamage = 0.15f;
		private static float bodySearchRadiusCoefficient = 5f;
		private static float intervalVariance = 0.5f;
		private static int baseTargets = 3;
		private static float orbDuration = 0.5f;

		private float durationFromBody;
		private float fireInterval;
		private float searchRadiusFromBody;
		private int targetsFromBody;

		private Queue<HurtBox> targetQueue;
		private float fireTimer;

		private TeamIndex killerTeamIndex;
		private GameObject killer;
		public override void OnEnter()
		{
			base.OnEnter();

			float radius = Mathf.Max(characterBody.radius, 1);

			// NEED CUSTOM SFX
			Util.PlayAttackSpeedSound("Play_golem_laser_fire", base.gameObject, 2f);
			if (DeathState.spawnEffectPrefab)
			{
				EffectManager.SpawnEffect(DeathState.spawnEffectPrefab, new EffectData
				{
					origin = base.characterBody.footPosition,
					scale = radius,
				}, false);
			}

			if (base.sfxLocator && base.sfxLocator.barkSound != "")
			{
				Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
			}

			Transform modelTransform = base.GetModelTransform();
			if (modelTransform)
				Destroy(modelTransform.gameObject);

			if (NetworkServer.active)
            {
				this.killer = healthComponent.lastHitAttacker;
				killerTeamIndex = TeamComponent.GetObjectTeam(killer);

				
				searchRadiusFromBody = radius * bodySearchRadiusCoefficient + searchRadius;
				targetsFromBody = baseTargets + Mathf.RoundToInt(radius - 1); // malachite also uses body radius sooo....
				durationFromBody = Mathf.RoundToInt(radius) * baseDuration;
				fireInterval = durationFromBody / targetsFromBody;
				SS2Log.Info($"DEATHSTATE. FROM BODY RADIUS {radius}, SEARCH={searchRadiusFromBody}, TARGETS={targetsFromBody}, DURATION={durationFromBody}");
				TeamMask mask = TeamMask.none;
				mask.AddTeam(base.teamComponent.teamIndex); // fuck it. its only targetting monsters. feels way better
				SphereSearch search = new SphereSearch
				{
					origin = base.characterBody.corePosition,
					radius = searchRadiusFromBody,
					mask = LayerIndex.entityPrecise.mask,
					queryTriggerInteraction = QueryTriggerInteraction.Ignore,
				}.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities();
				HurtBox[] hurtBoxes = search.GetHurtBoxes();
				targetQueue = new Queue<HurtBox>();
				for (int i = 0; i < hurtBoxes.Length - 1; i++)
				{
					SS2Log.Error("FOUND " + hurtBoxes[i].healthComponent.name);
					HurtBox hurtBox = hurtBoxes[i];
					if(hurtBox.healthComponent != base.healthComponent)
						targetQueue.Enqueue(hurtBox);

					if (targetQueue.Count >= targetsFromBody) break;
				}				
			}
			
		}

        public override void FixedUpdate()
        {
            base.FixedUpdate();
			if (rigidbody) rigidbody.velocity = Vector3.zero;
			if (characterMotor) characterMotor.velocity = Vector3.zero;

			if (!NetworkServer.active) return;
            
			fireTimer -= Time.fixedDeltaTime;
			if (fireTimer <= 0f)
			{
				float variance = fireInterval * intervalVariance;
				fireTimer += Random.Range(fireInterval - variance, fireInterval);
				FireLightning();
			}

			if (base.fixedAge >= durationFromBody || targetQueue.Count == 0)
            {
				Destroy(base.gameObject);
            }
			
			
        }


		/// NEEEEED A MINI SOUND FOR PLACING LIGHTNING
		private void FireLightning()
        {
			if (targetQueue.Count == 0) return;
			TeamIndex teamIndex = killerTeamIndex;
			if (teamIndex == TeamIndex.None) teamIndex = TeamIndex.Neutral;

			HurtBox hurtBox = targetQueue.Dequeue();
			SS2Log.Warning("FIRING AT " + hurtBox.healthComponent.name);
			float damage = characterBody.damage * damageCoefficient + characterBody.healthComponent.fullHealth * percentHealthDamage;
			OrbManager.instance.AddOrb(new AffixStormStrikeOrb
			{
				attacker = this.killer, // SHOULD WE LET KILLER PROC ITEMS FROM THIS DAMAGE????????????????????????????????
				damageColorIndex = DamageColorIndex.Default,
				teamIndex = teamIndex,
				damageValue = damage,
				isCrit = Util.CheckRoll(this.characterBody.crit, this.characterBody.master),
				procChainMask = default(ProcChainMask),
				damageType = DamageType.Shock5s,
				procCoefficient = .6f,
				target = hurtBox
			});
		}

        public override void OnExit()
        {
            base.OnExit();
			Destroy(base.gameObject);			
        }

        public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}


		public class AffixStormStrikeOrb : GenericDamageOrb, IOrbFixedUpdateBehavior
		{
			private Vector3 lastKnownTargetPosition;
			public override void Begin()
			{
				base.Begin();
				base.duration = orbDuration;
			}

            public void FixedUpdate()
            {
				if (this.target)
				{
					this.lastKnownTargetPosition = this.target.transform.position;
				}
			}

            public override GameObject GetOrbEffect()
			{
				return LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect");
			}

			public override void OnArrival()
			{
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/LightningStrikeImpact"), new EffectData
				{
					origin = this.lastKnownTargetPosition
				}, true);

				new BlastAttack
				{
					attacker = this.attacker,
					baseDamage = this.damageValue,
					baseForce = 0f,
					bonusForce = Vector3.down * 3000f,
					crit = this.isCrit,
					damageColorIndex = DamageColorIndex.Default,
					damageType = DamageType.Shock5s,
					falloffModel = BlastAttack.FalloffModel.Linear,
					inflictor = null,
					position = this.lastKnownTargetPosition,
					procChainMask = this.procChainMask,
					procCoefficient = 0.6f,
					radius = 3f,
					teamIndex = this.teamIndex
				}.Fire();
				
			}
			
		}
	}
}
