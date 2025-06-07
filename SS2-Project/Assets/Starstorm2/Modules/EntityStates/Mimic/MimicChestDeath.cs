using RoR2;
using SS2;
using SS2.Components;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Mimic
{
    public class MimicChestDeath : GenericCharacterDeath
    {
        public override void OnEnter()
        {
			base.OnEnter();
			//base.OnEnter();
			//this.bodyMarkedForDestructionServer = false;
			//this.cachedModelTransform = (base.modelLocator ? base.modelLocator.modelTransform : null);
			//this.isBrittle = (base.characterBody && base.characterBody.isGlass);
			//this.isVoidDeath = (base.healthComponent && (base.healthComponent.killingDamageType & DamageType.VoidDeath) > 0UL);
			//this.isPlayerDeath = (base.characterBody.master && base.characterBody.master.GetComponent<PlayerCharacterMasterController>() != null);
			//if (this.isVoidDeath)
			//{
			//	if (base.characterBody && base.isAuthority)
			//	{
			//		EffectManager.SpawnEffect(GenericCharacterDeath.voidDeathEffect, new EffectData
			//		{
			//			origin = base.characterBody.corePosition,
			//			scale = base.characterBody.bestFitRadius
			//		}, true);
			//	}
			//	if (this.cachedModelTransform)
			//	{
			//		EntityState.Destroy(this.cachedModelTransform.gameObject);
			//		this.cachedModelTransform = null;
			//	}
			//}
			//if (this.isPlayerDeath && base.characterBody)
			//{
			//	UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/PlayerDeathEffect"), base.characterBody.corePosition, Quaternion.identity).GetComponent<LocalCameraEffect>().targetCharacter = base.characterBody.gameObject;
			//}
			//if (this.cachedModelTransform)
			//{
			//	if (this.isBrittle)
			//	{
			//		TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(this.cachedModelTransform.gameObject);
			//		temporaryOverlayInstance.duration = 0.5f;
			//		temporaryOverlayInstance.destroyObjectOnEnd = true;
			//		temporaryOverlayInstance.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matShatteredGlass");
			//		temporaryOverlayInstance.destroyEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BrittleDeath");
			//		temporaryOverlayInstance.destroyEffectChildString = "Chest";
			//		temporaryOverlayInstance.inspectorCharacterModel = this.cachedModelTransform.gameObject.GetComponent<CharacterModel>();
			//		temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			//		temporaryOverlayInstance.animateShaderAlpha = true;
			//		temporaryOverlayInstance.transmit = false;
			//	}
			//	if (base.cameraTargetParams)
			//	{
			//		ChildLocator component = this.cachedModelTransform.GetComponent<ChildLocator>();
			//		if (component)
			//		{
			//			Transform transform = component.FindChild("Chest");
			//			if (transform)
			//			{
			//				base.cameraTargetParams.cameraPivotTransform = transform;
			//				this.aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
			//				base.cameraTargetParams.dontRaycastToPivot = true;
			//			}
			//		}
			//	}
			//}
			//if (!this.isVoidDeath)
			//{
			//	this.PlayDeathSound();
			//	this.PlayDeathAnimation(0.1f);
			//	this.CreateDeathEffects();
			//}

			if (gameObject)
            {
               var mim = gameObject.GetComponent<MimicInventoryManager>();
               SS2Log.Error("Death enter : " + mim + " | ");
               if (mim)
               {
                   SS2Log.Error("calling : " + mim + " | ");
                   mim.BeginCountdown();
               }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            SS2Log.Error("Death Exit : " + gameObject);

            base.OnExit();
        }
    }
}
