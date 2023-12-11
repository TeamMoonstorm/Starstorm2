using System;
using EntityStates;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace EntityStates.Chirr
{
	public class DroppedState : BaseState
	{
		public static float bounceForce = 2000f;
		public static float force = 800f;
		public static float blastRadius = 10f;
		public static float procCoefficient = 1f;
		public static float damageCoefficient = 10f;
		public GameObject inflictor;
		public Vector3 initialVelocity;
		public static GameObject hitGroundEffect = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardGroundSlam.prefab").WaitForCompletion();

		public float extraGravity;
		public bool friendlyDrop;

		public bool detonateNextFrame;

		private Collider[] colliders;
		public static float disableColliderDuration = 0.5f;
		private Collider inflictorCollider;
		private bool collidersEnabled;
		private DetonateOnImpact detonateOnImpact;
		private bool bodyHadGravity = true;
		public override void OnEnter()
		{
			base.OnEnter();
			Animator modelAnimator = base.GetModelAnimator();

			if (base.characterBody && NetworkServer.active) base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

			if (modelAnimator)
			{
				int layerIndex = modelAnimator.GetLayerIndex("Body");
				modelAnimator.enabled = false;
				modelAnimator.CrossFadeInFixedTime((UnityEngine.Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2", 0.1f);
				modelAnimator.Update(0f);
			}

			// figure out if we should disable gravity on exit
			GameObject prefab = BodyCatalog.GetBodyPrefab(this.characterBody.bodyIndex);
			if (prefab)
			{
				Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
				if (rigidbody) this.bodyHadGravity = rigidbody.useGravity;

			}

			//disable colliders to avoid colliding with dropper
			if (this.inflictor) this.inflictorCollider = this.inflictor.GetComponent<Collider>();
			this.colliders = base.gameObject.GetComponentsInChildren<Collider>();
			foreach(Collider collider in colliders)
            {
				collider.enabled = false;
				if (this.inflictorCollider) Physics.IgnoreCollision(collider, inflictorCollider, true);
            }

			if(base.characterMotor)
            {
                base.characterMotor.onMovementHit += DoSplashDamage;
				base.characterMotor.disableAirControlUntilCollision = true;
				base.characterMotor.velocity = initialVelocity;
            }
			else
            {				
				base.rigidbody.velocity = initialVelocity;
				base.rigidbody.useGravity = true;
				base.gameObject.AddComponent<DetonateOnImpact>().droppedState = this;
			}

		}

        private void DoSplashDamage(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
			this.detonateNextFrame = true;
		}

        public override void OnExit()
		{
			if(base.characterMotor)
            {
				base.characterMotor.onMovementHit -= DoSplashDamage;
            }

			if (this.detonateOnImpact) Destroy(this.detonateOnImpact);

			base.rigidbody.useGravity = bodyHadGravity;

			foreach (Collider collider in colliders)
			{
				collider.enabled = true;
				if (this.inflictorCollider) Physics.IgnoreCollision(collider, inflictorCollider, false);
			}
					
			if (NetworkServer.active) base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

			Animator modelAnimator = base.GetModelAnimator();
			if (modelAnimator)
			{
				modelAnimator.enabled = true;
			}
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if(base.fixedAge >= disableColliderDuration && !this.collidersEnabled) // idk what else to do
            {
				foreach (Collider collider in colliders)
				{
					collider.enabled = true;
				}
			}
			if (base.characterMotor)
			{
				base.characterMotor.velocity += Vector3.up * extraGravity * Time.fixedDeltaTime;
			}
			else
            {
				base.rigidbody.velocity += Vector3.up * extraGravity * Time.fixedDeltaTime;
			}

			if (detonateNextFrame && (!base.characterMotor || (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
            {
				if(base.characterMotor) 
					base.characterMotor.velocity = Vector3.zero;
				else 
					base.rigidbody.velocity = Vector3.zero;
				
				Util.PlaySound("Hit2", base.gameObject);
				if (NetworkServer.active)
				{
					EffectManager.SpawnEffect(hitGroundEffect, new EffectData
					{
						origin = base.characterBody.footPosition,
						scale = 1.25f,
					}, true);


					if (this.inflictor)
					{
						CharacterBody inflictorBody = this.inflictor.GetComponent<CharacterBody>();
						float damageStat = inflictorBody ? inflictorBody.damage : 12f;

						BlastAttack blastAttack = new BlastAttack();
						blastAttack.position = base.characterBody.footPosition;
						blastAttack.baseDamage = damageCoefficient * damageStat;
						blastAttack.baseForce = force;
						blastAttack.bonusForce = Vector3.up * bounceForce;
						blastAttack.radius = blastRadius;
						blastAttack.attacker = this.inflictor;
						blastAttack.inflictor = this.inflictor;
						blastAttack.teamIndex = inflictorBody.teamComponent.teamIndex;
						blastAttack.crit = inflictorBody.RollCrit();
						blastAttack.procChainMask = default(ProcChainMask);
						blastAttack.procCoefficient = procCoefficient;
						blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
						blastAttack.damageColorIndex = DamageColorIndex.Default;
						blastAttack.damageType = DamageType.Stun1s;
						blastAttack.attackerFiltering = AttackerFiltering.Default;
						//blastAttack.impactEffect = 
						BlastAttack.Result result = blastAttack.Fire();

					};
				}

				this.outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return friendlyDrop ? InterruptPriority.Skill : InterruptPriority.Vehicle; //////////////////////////////////
		}


		public class DetonateOnImpact : MonoBehaviour
        {
			public DroppedState droppedState;
            private void OnCollisionEnter(Collision collision)
            {
                if(collision.gameObject.layer == LayerIndex.world.intVal)
                {
					this.droppedState.detonateNextFrame = true;
					Destroy(this);
                }
            }
        }
	}
}