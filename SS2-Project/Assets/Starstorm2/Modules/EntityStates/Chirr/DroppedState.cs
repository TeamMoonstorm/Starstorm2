using System;
using EntityStates;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;
namespace EntityStates.Chirr
{
	// probably worth to turn this into a general-use "body launch" state, and set the parameters when instantiating the state.
	public class DroppedState : BaseState
	{
		public static float bounceForce = 2000f;
		public static float force = 800f;
		public static float blastRadius = 10f;
		public static float procCoefficient = 1f;
		public static float damageCoefficient = 15f;
		public static float absoluteMaxTime = 8f;
		public GameObject inflictor;
		public Vector3 initialVelocity;
		public static GameObject hitGroundEffect = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardGroundSlam.prefab").WaitForCompletion();

		public float extraGravity;
		public bool friendlyDrop;

		public bool detonateNextFrame;

		private DetonateOnImpact detonateOnImpact;
		private bool bodyHadGravity = true;
		private bool bodyWasKinematic = true;
		private bool bodyCouldTakeImpactDamage = true;
		private CharacterGravityParameters gravParams;
		private Rigidbody tempRigidbody;
		private SphereCollider tempSphereCollider;
		public override void OnEnter()
		{
			base.OnEnter();
			Animator modelAnimator = base.GetModelAnimator();
			if (base.characterBody && NetworkServer.active) base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

			if (modelAnimator)
			{
				if(!friendlyDrop)
                {
					int layerIndex = modelAnimator.GetLayerIndex("Body");
					modelAnimator.enabled = false;
					modelAnimator.CrossFadeInFixedTime((UnityEngine.Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2", 0.1f);
					modelAnimator.Update(0f);
				}
				else
                {
					AimAnimator aimAnimator = base.GetAimAnimator();
					if (aimAnimator) aimAnimator.enabled = true; // exiting genericcharactermain disables it
				}					
				
			}

			// figure out if we should disable gravity/kinematic on exit
			GameObject prefab = BodyCatalog.GetBodyPrefab(this.characterBody.bodyIndex);
			if (prefab)
			{
				Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
				if (rigidbody)
				{
					this.bodyHadGravity = rigidbody.useGravity;
					this.bodyWasKinematic = rigidbody.isKinematic;
				}
			}

			
			if(base.characterMotor)
            {
                base.characterMotor.onMovementHit += DoSplashDamage;
				base.characterMotor.disableAirControlUntilCollision = true;
				base.characterMotor.velocity = initialVelocity;
				//blind pests have low gravity
				this.gravParams = base.characterMotor.gravityParameters;
				base.characterMotor.gravityParameters = new CharacterGravityParameters 
				{
					environmentalAntiGravityGranterCount = 0,
					antiGravityNeutralizerCount = 0,
					channeledAntiGravityGranterCount = 0,
				}; // ??????????????????????????????????????????????????????????
            }
			else
            {
				Rigidbody rigidbody = base.rigidbody; // CONSTRUCTS AND SHIT DONT HAVE RIGIDBODIES
				if(!rigidbody)
                {
					rigidbody = base.gameObject.AddComponent<Rigidbody>();
					this.tempRigidbody = rigidbody;
					this.tempSphereCollider = base.gameObject.AddComponent<SphereCollider>();
                }
				rigidbody.velocity = initialVelocity;
				rigidbody.useGravity = true;
				//rigidbody.isKinematic = true; // maybe we should force shit downwards instead of lettign gravity do it
				this.detonateOnImpact = base.gameObject.AddComponent<DetonateOnImpact>();
				this.detonateOnImpact.droppedState = this;
				if(base.rigidbodyMotor)
                {
					this.bodyCouldTakeImpactDamage = base.rigidbodyMotor.canTakeImpactDamage;
					base.rigidbodyMotor.canTakeImpactDamage = false;
					base.rigidbodyMotor.enabled = false;
                }
			}

			if(base.isAuthority)
            {
				GrabController gc = this.inflictor.GetComponent<GrabController>();
				gc.onVictimReleased += ReleaseLatencyFixTEMP;
			}
			
		}

		// the syncvarhook for GrabController.victimBodyObject, which ends the grab, can take longer than the rpc call in ChirrGrabBehavior, which sets the victim to DroppedState.
		// which means that DroppedState.OnEnter might happen before GrabController.EndGrab
		// and velocity is forced to 0 while grabbed
		// so this is a jank "fix" to re-set our velocity after entering this state AND after the grab ends.
		// ^^ im so bad at networking T_T 
		// ^^^ would using another RPC call instead of syncvarhook for victimBodyObject make the timing correct?
		private void ReleaseLatencyFixTEMP(VehicleSeat.PassengerInfo _) 
        {
			Moonstorm.Starstorm2.SS2Log.Warning("ReleaseLatencyFixTEMP: Setting velocity to " + this.initialVelocity);
			if (base.characterMotor)
				base.characterMotor.velocity = initialVelocity;
			else if (base.rigidbody)
				base.rigidbody.velocity = initialVelocity;
        }

        private void DoSplashDamage(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
			this.detonateNextFrame = true;
		}

        public override void OnExit()
		{
			if (base.isAuthority)
			{
				GrabController gc = this.inflictor.GetComponent<GrabController>();
				gc.onVictimReleased -= ReleaseLatencyFixTEMP;
			}

			if (base.characterMotor)
            {
				base.characterMotor.onMovementHit -= DoSplashDamage;
				base.characterMotor.gravityParameters = this.gravParams;
            }

			if (this.detonateOnImpact) Destroy(this.detonateOnImpact);
			if (this.tempRigidbody) Destroy(this.tempRigidbody);
			if (this.tempSphereCollider) Destroy(this.tempSphereCollider);

			if (base.rigidbodyMotor)
			{
				base.rigidbodyMotor.canTakeImpactDamage = this.bodyCouldTakeImpactDamage;
				base.rigidbodyMotor.enabled = true;
			}
			if(base.rigidbody)
            {
				base.rigidbody.useGravity = bodyHadGravity;
				base.rigidbody.isKinematic = bodyWasKinematic;
			}		

			if (NetworkServer.active) base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage; // fuck u loader

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

			// let teammates do inputs while falling 
			if (friendlyDrop && base.isLocalPlayer)
            {
				this.PerformInputs();
            }

			if (base.characterMotor)
			{
				base.characterMotor.velocity += Vector3.up * extraGravity * Time.fixedDeltaTime;
			}
			else
            {
				Rigidbody rigidbody = base.rigidbody ? base.rigidbody : this.tempRigidbody;
				rigidbody.velocity += Vector3.up * extraGravity * Time.fixedDeltaTime;
			}

			// should find out why stuff can get stuck floating despite gravity not being disabled...
			if (base.fixedAge > absoluteMaxTime || detonateNextFrame && (!base.characterMotor || (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
            {
				if (base.characterMotor)
					base.characterMotor.velocity = Vector3.zero;
				else if (base.rigidbody)
					base.rigidbody.velocity = Vector3.zero;
				else if (this.tempRigidbody)
					this.tempRigidbody.velocity = Vector3.zero;

				Util.PlaySound("ChirrThrowHitGround", base.gameObject);
				if (base.isAuthority) // authority because server doesnt see clients hitting the ground
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

		protected void PerformInputs()
		{
			if (base.isAuthority && base.skillLocator)
			{
				this.PerformInput(base.skillLocator.primary, ref base.inputBank.skill1);
				this.PerformInput(base.skillLocator.secondary, ref base.inputBank.skill2);
				this.PerformInput(base.skillLocator.utility, ref base.inputBank.skill3);
				this.PerformInput(base.skillLocator.special, ref base.inputBank.skill4);				
			}
		}
		private void PerformInput(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
		{
			if (!buttonState.down || !skillSlot)
			{
				return;
			}
			if (skillSlot.mustKeyPress && buttonState.hasPressBeenClaimed)
			{
				return;
			}
			if (skillSlot.ExecuteIfReady())
			{
				buttonState.hasPressBeenClaimed = true;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return friendlyDrop ? InterruptPriority.Skill : InterruptPriority.Vehicle; //////////////////////////////////
		}

        public override void OnSerialize(NetworkWriter writer) // njot necessary. chirrgrabbehavior handles this
        {
            base.OnSerialize(writer);
			writer.Write(this.initialVelocity);
			writer.Write(this.friendlyDrop);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
			this.initialVelocity = reader.ReadVector3();
			this.friendlyDrop = reader.ReadBoolean();
        }


        public class DetonateOnImpact : MonoBehaviour
        {
			public DroppedState droppedState;
            private void OnCollisionEnter(Collision collision)
            {
                if(collision.gameObject.layer == LayerIndex.world.intVal || collision.gameObject.layer == LayerIndex.entityPrecise.intVal)
                {
					this.droppedState.detonateNextFrame = true;
					Destroy(this);
                }
            }
        }
	}
}