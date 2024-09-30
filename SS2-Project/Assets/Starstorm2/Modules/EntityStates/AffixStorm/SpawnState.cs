using RoR2;
using UnityEngine;
using SS2;
namespace EntityStates.AffixStorm
{
	public class SpawnState : BaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();

			// NEED CUSTOM SFX
			//base.PlayAnimation("Body", "Idle");
			if (SpawnState.spawnEffectPrefab)
			{
				GameObject effect = SpawnState.spawnEffectPrefab;
				if (Storm.brightStages.Contains(R2API.DirectorAPI.GetStageEnumFromSceneDef(RoR2.Stage.instance.sceneDef))) effect = spawnBrightEffectPrefab;
				EffectManager.SpawnEffect(effect, new EffectData
				{
					origin = base.characterBody.corePosition,
					scale = base.characterBody.radius,
				}, false);
			}

			Transform modelTransform = base.GetModelTransform();
			if (modelTransform)
			{
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = 0.6f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matLightningSpawn", SS2Bundle.Equipments);

                // TODO: No longer needed post-SOTS, leaving in for now but need to remove later
                // temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());


                TemporaryOverlayInstance temporaryOverlay2 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay2.duration = 0.7f;
				temporaryOverlay2.animateShaderAlpha = true;
				temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay2.destroyComponentOnEnd = true;
				temporaryOverlay2.originalMaterial = SS2Assets.LoadAsset<Material>("matLightningSpawnExpanded", SS2Bundle.Equipments);

                // TODO: No longer needed post-SOTS, leaving in for now but need to remove later
                // temporaryOverlay2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
			}

			
		}

        public override void FixedUpdate()
        {
            base.FixedUpdate();
			if (fixedAge > duration)
				this.outer.SetNextStateToMain();
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
			return InterruptPriority.Death;
        }
		public static float duration = 0.5f;

		public static GameObject spawnBrightEffectPrefab;
		public static GameObject spawnEffectPrefab;
	}
}
