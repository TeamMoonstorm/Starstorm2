using RoR2;
using UnityEngine;
using SS2;
using UnityEngine.Networking;
namespace EntityStates.AffixEmpyrean
{
	public class SpawnState : BaseState
	{
		private Animator animator;
		private AimAnimator aimAnimator;
		public override void OnEnter()
		{
			base.OnEnter();

			// NEED CUSTOM SFX

			float minX = Mathf.Infinity;
			float maxX = Mathf.NegativeInfinity;
			float minZ = Mathf.Infinity;
			float maxZ = Mathf.NegativeInfinity;
			foreach (HurtBox h in characterBody.hurtBoxGroup.hurtBoxes)
			{

				if (h.collider.bounds.min.x < minX)
					minX = h.collider.bounds.min.x;
				if (h.collider.bounds.max.x > maxX)
					maxX = h.collider.bounds.max.x;

				if (h.collider.bounds.min.z < minZ)
					minZ = h.collider.bounds.min.z;
				if (h.collider.bounds.max.z > maxZ)
					maxZ = h.collider.bounds.max.z;
			}
			minX -= characterBody.corePosition.x;
			maxX -= characterBody.corePosition.x;
			minZ -= characterBody.corePosition.z;
			maxZ -= characterBody.corePosition.z;

			float scale = Mathf.Max(minX, maxX, minZ, maxZ);
			scale = Mathf.Max(scale, 1);
			int sizeBoost = characterBody.inventory.GetItemCount(SS2Content.Items.BoostCharacterSize);
			scale *= (1 + sizeBoost / 100f);
			this.animator = base.GetModelAnimator();
			if(animator)
            {
				base.PlayAnimation("Body", "Idle");
				animator.Play("Idle", animator.GetLayerIndex("Body"), .5f); // ok just dont do anything i guess. just tpose. whatever
				animator.SetBool("isGrounded", true);
				animator.SetFloat("aimPitchCycle", .5f);
				animator.SetFloat("aimYawCycle", .5f);			
			}
			aimAnimator = base.GetAimAnimator();
			if (aimAnimator)
			{
				aimAnimator.enabled = false;
			}

			if (NetworkServer.active && SpawnState.spawnEffectPrefab)
			{
				EffectManager.SpawnEffect(SpawnState.spawnEffectPrefab, new EffectData
				{
					origin = base.characterBody.isFlying ? characterBody.corePosition : characterBody.footPosition,
					scale = scale,
				}, true); // transmit was false but it still played twice ???????????????????????????????????????
			}

			characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreKnockback;
			
			Transform modelTransform = base.GetModelTransform();
			if (modelTransform)
			{
				TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
				temporaryOverlay.duration = 3f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.Linear(0f, 0.3f, 1f, 1f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matRainbowSpawnOverlay2", SS2Bundle.Equipments);
				temporaryOverlay.AddToCharacterModel(modelTransform.gameObject.GetComponent<CharacterModel>());

				var pc = modelTransform.GetComponent<PrintController>();
				if (pc) pc.enabled = false;
			}

		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			animator.enabled = false;
			if (fixedAge > duration)
				this.outer.SetNextStateToMain();
		}

        public override void OnExit()
        {
            base.OnExit();
			Transform modelTransform = base.GetModelTransform();
			if(modelTransform)
            {
				TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
				temporaryOverlay.duration = .3f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matRainbowSpawnOverlayBright", SS2Bundle.Equipments);
				temporaryOverlay.AddToCharacterModel(modelTransform.gameObject.GetComponent<CharacterModel>());

			}

			characterBody.bodyFlags &= CharacterBody.BodyFlags.IgnoreKnockback;
			if (animator) animator.enabled = true;
			if (aimAnimator)
			{
				aimAnimator.enabled = true;
			}
		}


        public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}
		public static float duration = 2.95f;

		public static GameObject spawnEffectPrefab;
	}
}
