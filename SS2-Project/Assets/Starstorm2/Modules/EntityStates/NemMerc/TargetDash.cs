using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Merc;
using Moonstorm.Starstorm2.Components;
using R2API;
using Moonstorm.Starstorm2;

namespace EntityStates.NemMerc
{
	//copy pasted merc assaulter
	public class TargetDash : BaseSkillState
	{
		public bool hasHit;
		public bool dashVectorLocked;
		public GameObject target;

		public bool m2Buffered;

		private EntityStateMachine weapon;
		private int entryLayer;
		public override void OnEnter()
		{
			base.OnEnter();
			Util.PlaySound(TargetDash.beginSoundString, base.gameObject);
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
            }

			this.overlapAttack = base.InitMeleeOverlap(TargetDash.damageCoefficient, TargetDash.hitEffectPrefab, this.modelTransform, "Assaulter");
			this.overlapAttack.damageType = DamageType.Stun1s;
			this.overlapAttack.AddModdedDamageType(Moonstorm.Starstorm2.DamageTypes.RedirectHologram.damageType);

			Vector3 dashTarget = base.GetAimRay().GetPoint(TargetDash.baseDashDistance);

			if(this.target)
            {
				Vector3 between = target.transform.position - base.transform.position;
				float distance = between.magnitude;
				Vector3 direction = between.normalized;
				dashTarget = direction * (distance + TargetDash.dashBehindDistance) + base.transform.position;
				if (Physics.Raycast(target.transform.position, direction, out RaycastHit hit, TargetDash.dashBehindDistance, LayerIndex.world.mask))
				{
					dashTarget = hit.point;
				}
			}

			Vector3 between2 = dashTarget - base.transform.position;

			this.dashSpeed = between2.magnitude / TargetDash.dashDuration;// / this.attackSpeedStat);
			this.dashVector = between2.normalized;

			base.SmallHop(base.characterMotor, TargetDash.smallHopVelocity);

			//reuse assaulter anim
			this.CreateDashEffect();
			base.PlayCrossfade("FullBody, Override", "AssaulterLoop", 0.1f);

			this.entryLayer = base.gameObject.layer;
			base.gameObject.layer = this.target ? LayerIndex.noCollision.intVal : LayerIndex.debris.intVal;
			base.characterMotor.Motor.RebuildCollidableLayers();

			if (this.modelTransform)
			{
				TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 1.2f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matNemergize", SS2Bundle.NemMercenary);
				temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
			}

			
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
			if (transform && TargetDash.dashPrefab)
			{
				UnityEngine.Object.Instantiate<GameObject>(TargetDash.dashPrefab, transform.position, Util.QuaternionSafeLookRotation(this.dashVector), transform);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			this.GatherInputs();

			base.characterDirection.forward = this.dashVector;

			if (base.isAuthority)
			{
				base.characterMotor.velocity = Vector3.zero;
				if (!this.inHitPause)
				{
					this.stopwatch += Time.fixedDeltaTime;
					if (this.overlapAttack.Fire())
					{
						if (!this.hasHit)
						{
							this.hasHit = true;
						}
						this.inHitPause = true;
						this.hitPauseTimer = TargetDash.hitPauseDuration / this.attackSpeedStat;

						if (this.m2Buffered)
						{
							this.m2Buffered = false;
							base.skillLocator.secondary.DeductStock(1);
							this.hitPauseTimer += TargetDash.m2HitPauseDuration / this.attackSpeedStat;
							this.inM2HitPause = true;

							// prevent clipping when redirecting/extending the dash
							base.gameObject.layer = LayerIndex.fakeActor.intVal;
							base.characterMotor.Motor.RebuildCollidableLayers();

							if (this.weapon) this.weapon.SetNextState(new WhirlwindAssaulter());
						}

						if (this.modelTransform)
						{
							TemporaryOverlay temporaryOverlay2 = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
							temporaryOverlay2.duration = TargetDash.hitPauseDuration / this.attackSpeedStat;
							temporaryOverlay2.animateShaderAlpha = true;
							temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
							temporaryOverlay2.destroyComponentOnEnd = true;
							temporaryOverlay2.originalMaterial = SS2Assets.LoadAsset<Material>("matNemergize", SS2Bundle.NemMercenary);
							temporaryOverlay2.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
						}
					}
					base.characterMotor.rootMotion += this.dashVector * this.dashSpeed * Time.fixedDeltaTime;
				}
				else
				{
					//if (this.inM2HitPause) this.dashVector = base.inputBank.aimDirection;

					this.hitPauseTimer -= Time.fixedDeltaTime;
					if (this.hitPauseTimer < 0f)
					{
						this.inM2HitPause = false;
						this.inHitPause = false;
					}
				}
			}
			if (this.stopwatch >= TargetDash.dashDuration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.gameObject.layer = this.entryLayer;
			base.characterMotor.Motor.RebuildCollidableLayers();
			Util.PlaySound(TargetDash.endSoundString, base.gameObject);
			if (base.isAuthority)
			{
				base.characterMotor.velocity *= 0.1f;
				base.SmallHop(base.characterMotor, TargetDash.smallHopVelocity);
			}
			CameraTargetParams.AimRequest aimRequest = this.aimRequest;
			if (aimRequest != null)
			{
				aimRequest.Dispose();
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
			writer.Write(this.target);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			this.target = reader.ReadGameObject();
		}


		[NonSerialized]
		public Transform modelTransform;

		public static GameObject dashPrefab;

		public static float smallHopVelocity;

		public static float m2HitPauseDuration = 0.33f;

		public static float dashDuration = 0.2f;

		public static float baseDashDistance = 20f;

		public static float dashBehindDistance = 5f;

		public static string beginSoundString;

		public static string endSoundString;

		public static float damageCoefficient = 3f;

		public static float procCoefficient = 1;

		public static GameObject hitEffectPrefab;

		public static float hitPauseDuration = 0.15f;

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
		public bool inHitPause;
		[NonSerialized]
		public bool inM2HitPause;

		[NonSerialized]
		public float hitPauseTimer;

		[NonSerialized]
		public CameraTargetParams.AimRequest aimRequest;
	}
}
