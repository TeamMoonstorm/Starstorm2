using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using Moonstorm.Starstorm2;
namespace EntityStates.NemMerc
{

	//copy pasted merc assaulter
	public class NemAssaulter : BaseState
	{
		public bool hasHit;
		public bool dashVectorLocked;
		public HurtBox target;

		private bool m2Buffered;

        private EntityStateMachine weapon;

		public override void OnEnter()
		{
			base.OnEnter();
			Util.PlaySound(NemAssaulter.beginSoundString, base.gameObject);
			this.modelTransform = base.GetModelTransform();

			this.weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");

			if (base.cameraTargetParams)
			{
				this.aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
			}
			
			if (this.modelTransform)
			{
				this.animator = this.modelTransform.GetComponent<Animator>();
				this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
				this.childLocator = this.modelTransform.GetComponent<ChildLocator>();
				this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
                if (this.childLocator)
                {
                    this.childLocator.FindChild("PreDashEffect").gameObject.SetActive(true);
                }

				TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = NemAssaulter.dashPrepDuration;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matNemergize", SS2Bundle.NemMercenary);
				temporaryOverlay.AddToCharacerModel(this.characterModel);
				
			}

			float distance = NemAssaulter.dashDistance + minBehindDistance;

			this.dashVector = base.inputBank.aimDirection;

			if(this.target)
            {
				Vector3 between = this.target.transform.position - base.transform.position;
				distance = Mathf.Min(between.magnitude + minBehindDistance, distance + graceDistance); // extra distance incase enemy wanders away before the dash
			}				
			

			base.characterDirection.forward = this.dashVector.normalized;

			distance = Mathf.Max(distance, NemAssaulter.dashDistance);
			this.dashSpeed = distance / (NemAssaulter.dashDuration);// / this.attackSpeedStat);
			//this.dashSpeed *= this.attackSpeedStat;

			base.SmallHop(base.characterMotor, NemAssaulter.smallHopVelocity);
			base.PlayAnimation("FullBody, Override", "AssaulterPrep", "AssaulterPrep.playbackRate", NemAssaulter.dashPrepDuration);

			
			
			this.overlapAttack = base.InitMeleeOverlap(NemAssaulter.damageCoefficient, NemAssaulter.hitEffectPrefab, this.modelTransform, "Assaulter");
			this.overlapAttack.damageType = DamageType.Stun1s;
			if (NetworkServer.active)
			{
				base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
			}

		}

		private void GatherInputs()
        {
			GenericSkill secondary = base.skillLocator.secondary;
			//nametoken is stupid and lazy
			if (base.inputBank.skill2.justPressed && secondary.IsReady() 
				&& secondary.skillDef.skillNameToken == "SS2_NEMESIS_MERCENARY_SECONDARY_SLASH_NAME")
            {				
				this.m2Buffered = true;

				//vfx and sound
            }
        }

        public void CreateDashEffect()
		{
			Transform transform = this.childLocator.FindChild("DashCenter");
			Util.PlaySound("Play_nemmerc_dash", base.gameObject);
			if (transform && NemAssaulter.dashPrefab)
			{
				UnityEngine.Object.Instantiate<GameObject>(NemAssaulter.dashPrefab, transform.position, Util.QuaternionSafeLookRotation(this.dashVector), transform);
			}
            if (this.childLocator)
            {
                this.childLocator.FindChild("PreDashEffect").gameObject.SetActive(false);
            }
        }

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			this.GatherInputs();

			float totalDuration = NemAssaulter.dashDuration + NemAssaulter.dashPrepDuration;
			base.characterDirection.forward = this.dashVector; 
			if (this.stopwatch > NemAssaulter.dashPrepDuration && !this.isDashing)
			{
				this.isDashing = true;
				this.dashVector = base.inputBank.aimDirection;

				this.CreateDashEffect();
				base.PlayCrossfade("FullBody, Override", "AssaulterLoop", 0.1f);
				base.gameObject.layer = LayerIndex.fakeActor.intVal;
				base.characterMotor.Motor.RebuildCollidableLayers();
				if (this.modelTransform)
				{
					TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
					temporaryOverlay.duration = 1.2f;
					temporaryOverlay.animateShaderAlpha = true;
					temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlay.destroyComponentOnEnd = true;
					temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matNemergize", SS2Bundle.NemMercenary);
					temporaryOverlay.AddToCharacerModel(this.characterModel);
				}
			}
			if (!this.isDashing)
			{
				base.characterMotor.velocity = Vector3.zero;
				this.stopwatch += Time.fixedDeltaTime;
			}
			else if (base.isAuthority)
			{
				base.characterMotor.velocity = Vector3.zero;
				if (!this.inHitPause)
				{
					this.stopwatch += Time.fixedDeltaTime;
					if (this.overlapAttack.Fire(null))
					{
						if (!this.hasHit)
						{
							this.hasHit = true;
						}
						this.inHitPause = true;
						this.hitPauseTimer = NemAssaulter.hitPauseDuration / this.attackSpeedStat;

						if(this.m2Buffered)
                        {
							// NNEED A BETTER DASH START SOUND HERE
							Util.PlaySound(NemAssaulter.beginSoundString, base.gameObject); // THIS SUCKS I THINK

							this.m2Buffered = false;
							base.skillLocator.secondary.DeductStock(1);
							this.hitPauseTimer += NemAssaulter.m2HitPauseDuration / this.attackSpeedStat;
							this.inM2HitPause = true;

							//add extra time only to the END of the dash
							float extraTime = (this.stopwatch + NemAssaulter.m2ExtraDashDuration) - totalDuration;
							if(extraTime > 0)
								this.stopwatch -= extraTime;

							if (this.weapon) this.weapon.SetNextState(new WhirlwindAssaulter());
                        }

						if (this.modelTransform)
						{
							TemporaryOverlay temporaryOverlay2 = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
							temporaryOverlay2.duration = NemAssaulter.hitPauseDuration / this.attackSpeedStat;
							temporaryOverlay2.animateShaderAlpha = true;
							temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
							temporaryOverlay2.destroyComponentOnEnd = true;
							temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
							temporaryOverlay2.AddToCharacerModel(this.characterModel);
						}
					}
					base.characterMotor.rootMotion += this.dashVector * this.dashSpeed * Time.fixedDeltaTime;
				}
				else
				{
					if(this.inM2HitPause) this.dashVector = base.inputBank.aimDirection;

					this.hitPauseTimer -= Time.fixedDeltaTime;
					if (this.hitPauseTimer < 0f)
					{
						this.inM2HitPause = false;
						this.inHitPause = false;
					}
				}
			}
			if (this.stopwatch >= totalDuration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.gameObject.layer = LayerIndex.defaultLayer.intVal;
			base.characterMotor.Motor.RebuildCollidableLayers();
			Util.PlaySound(NemAssaulter.endSoundString, base.gameObject);
			if (base.isAuthority)
			{
				base.characterMotor.velocity *= 0.1f;
				base.SmallHop(base.characterMotor, NemAssaulter.smallHopVelocity);
			}
			CameraTargetParams.AimRequest aimRequest = this.aimRequest;
			if (aimRequest != null)
			{
				aimRequest.Dispose();
			}
            if (this.childLocator)
            {
                this.childLocator.FindChild("PreDashEffect").gameObject.SetActive(false);
            }
            this.PlayAnimation("FullBody, Override", "EvisLoopExit", "Special.playbackRate", 1f);
			if (NetworkServer.active)
			{
				base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
			}
			base.OnExit();
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
			return InterruptPriority.Pain;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
			writer.Write(HurtBoxReference.FromHurtBox(this.target));
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			this.target = reader.ReadHurtBoxReference().ResolveHurtBox();
		}

		[NonSerialized]
		public Transform modelTransform;

		public static GameObject dashPrefab;

		public static float smallHopVelocity;

		public static float m2ExtraDashDuration = 0.1f;

		public static float m2HitPauseDuration = 0.33f;

		public static float dashPrepDuration = 0.75f;

		public static float dashDistance = 35f;

		public static float dashDuration = 0.2f;

		public static string beginSoundString;

		public static string endSoundString;

		public static float damageCoefficient = 15f;

		public static float procCoefficient = 1;

		public static GameObject hitEffectPrefab;

		public static float hitPauseDuration = 0.15f;

		private static float minBehindDistance = 5f;

		private static float graceDistance = 10f;
		private float dashSpeed;

		[NonSerialized]
		public float stopwatch;

		[NonSerialized]
		public Vector3 dashVector = Vector3.zero;

		[NonSerialized]
		public Animator animator;

		[NonSerialized]
		public CharacterModel characterModel;

		[NonSerialized]
		public HurtBoxGroup hurtboxGroup;

		[NonSerialized]
		public OverlapAttack overlapAttack;

		[NonSerialized]
		public ChildLocator childLocator;

		[NonSerialized]
		public bool isDashing;

		[NonSerialized]
		public bool inHitPause;
		[NonSerialized]
		public bool inM2HitPause;

		[NonSerialized]
		public float hitPauseTimer;

		[NonSerialized]
		public CameraTargetParams.AimRequest aimRequest;
	}
}
