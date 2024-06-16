using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2;
namespace EntityStates.AffixStorm
{
	public class DeathState : BaseState
	{
		public static float duration = 2f;

		public static GameObject spawnEffectPrefab;
		public static GameObject blastEffectPrefab;

		private static float blastForce = 1600f;
		private static float blastRadius = 8f;
		private static float blastDamageCoefficient = 3f;
		private static float shockInterval = 0.1f;
		private static float shockStrength = 1f;

		private Animator animator;
		private Transform modelTransform;

		private float shockTimer;
		private float blastRadiusFromBody;
		public override void OnEnter()
		{
			base.OnEnter();

			// NEED CUSTOM SFX
			Util.PlayAttackSpeedSound("Play_vagrant_R_charge", base.gameObject, 2f);
			animator = this.GetModelAnimator();

			blastRadiusFromBody = base.characterBody.radius + blastRadius;


			if (DeathState.spawnEffectPrefab)
			{
				EffectManager.SpawnEffect(DeathState.spawnEffectPrefab, new EffectData
				{
					origin = base.characterBody.corePosition,
					scale = blastRadiusFromBody,
				}, false);
			}

			if (base.sfxLocator && base.sfxLocator.barkSound != "")
			{
				Util.PlaySound(base.sfxLocator.barkSound, base.gameObject);
			}

			this.modelTransform = base.GetModelTransform();
			if (modelTransform)
			{
				CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>();
				if (characterModel)
				{
					Material overlay = SS2Assets.LoadAsset<Material>("matAffixLightning", SS2Bundle.Equipments);
					// this is fine since the guy explodes right after
					for (int i = 0; i < characterModel.baseRendererInfos.Length - 1; i++)
                    {
						characterModel.baseRendererInfos[i].defaultMaterial = overlay;
                    }

					//characterModel.invisibilityCount++;

					TemporaryOverlay temporaryOverlay = base.gameObject.AddComponent<TemporaryOverlay>();
					temporaryOverlay.duration = 0.5f;
					temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matLightningSpawnExpanded", SS2Bundle.Equipments);
					temporaryOverlay.AddToCharacerModel(characterModel);
				}
			}


			if (rigidbody) rigidbody.useGravity = false;
			if (characterMotor) characterMotor.useGravity = false;

		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (rigidbody) rigidbody.velocity = Vector3.zero;
			if (characterMotor) characterMotor.velocity = Vector3.zero;
			if (this.shockTimer <= 0f)
			{
				this.shockTimer += shockInterval;
				this.PlayShockAnimation();
			}
			if (fixedAge > duration)
				this.outer.SetNextStateToMain();
		}


		private void PlayShockAnimation()
		{
			int layerIndex = this.animator.GetLayerIndex("Flinch");
			if (layerIndex >= 0)
			{
				this.animator.SetLayerWeight(layerIndex, shockStrength);
				this.animator.Play("FlinchStart", layerIndex);
			}
		}

        public override void OnExit()
        {
            base.OnExit();

			Destroy(this.modelTransform.gameObject);
			Destroy(base.gameObject);

			// CUSTOM SFX PLZZZZZZZZZZZZ
			Util.PlayAttackSpeedSound("Play_vagrant_R_explode", base.gameObject, 2f);

			if (NetworkServer.active)
            {
				CharacterBody characterBody = base.characterBody;
				EffectManager.SpawnEffect(blastEffectPrefab, new EffectData
				{
					origin = characterBody.corePosition,
					scale = blastRadiusFromBody,
				}, true);
				new BlastAttack
				{
					attacker = characterBody.gameObject,
					inflictor = characterBody.gameObject,
					attackerFiltering = AttackerFiltering.AlwaysHit,
					position = characterBody.corePosition,
					teamIndex = TeamIndex.Neutral,
					radius = blastRadiusFromBody,
					baseDamage = characterBody.damage * base.characterBody.damage,
					damageType = DamageType.Shock5s,
					crit = base.RollCrit(),
					procCoefficient = .4f,
					procChainMask = default(ProcChainMask),
					baseForce = 1600f,
					damageColorIndex = DamageColorIndex.Default,
					falloffModel = BlastAttack.FalloffModel.Linear,
					losType = BlastAttack.LoSType.NearestHit,
				}.Fire();
			}
			
        }

        public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}
		
	}
}
