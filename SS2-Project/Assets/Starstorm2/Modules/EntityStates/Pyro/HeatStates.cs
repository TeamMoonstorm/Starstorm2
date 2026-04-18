using UnityEngine;
using RoR2;
using SS2.Components;
using SS2;

namespace EntityStates.Pyro
{
    // for networking purposes, since heat is client auth.. needs sounds, vfx, anims
    public abstract class BaseHeatState : BaseState
    {
        protected PyroController pyroController;

        public override void OnEnter()
        {
            base.OnEnter();
            if (!TryGetComponent(out pyroController))
            {
                SS2Log.Error("BaseHeatState.OnEnter: PyroController missing");
            }
        }
    }
    public class LowHeat : BaseHeatState
    {
        private static string enterSoundString = "Play_item_void_bleedOnHit_explo";
        public static GameObject effectPrefab;
        public override void OnEnter()
        {
            base.OnEnter();
            if (pyroController) pyroController.SetHighHeat(false);
            Util.PlaySound(enterSoundString, gameObject);

            // TODO: figure out hwhat kind of effect this hsould be
            //EffectManager.SimpleEffect(effectPrefab, transform.position, Quaternion.identity, false);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && pyroController && pyroController.heat >= pyroController.highHeat)
            {
                outer.SetNextState(new HighHeat());
            }
        }
    }
    public class HighHeat : BaseHeatState
    {
        private static string enterSoundString = "Play_voidman_m2_shoot_fullCharge";
        public static GameObject effectPrefab;
        public static Material flashMaterial;
        public static Material overlayMaterial;

        private Transform modelTransform;
        private TemporaryOverlayInstance overlayInstance;
        public override void OnEnter()
        {
            base.OnEnter();
            if (pyroController) pyroController.SetHighHeat(true);
            Util.PlaySound(enterSoundString, gameObject);

            // TODO: figure out hwhat kind of effect this hsould be
            //EffectManager.SimpleEffect(effectPrefab, transform.position, Quaternion.identity, false);

            modelTransform = GetModelTransform();
            if (modelTransform)
            {
                var flashOverlayInstance = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                flashOverlayInstance.duration = 0.4f;
                flashOverlayInstance.animateShaderAlpha = true;
                flashOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                flashOverlayInstance.destroyComponentOnEnd = true;
                flashOverlayInstance.originalMaterial = flashMaterial;
                if (modelTransform.TryGetComponent(out CharacterModel flashCharModel))
                    flashOverlayInstance.AddToCharacterModel(flashCharModel);

                overlayInstance = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                overlayInstance.duration = 0.4f;
                overlayInstance.animateShaderAlpha = true;
                overlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                overlayInstance.destroyComponentOnEnd = true;
                overlayInstance.originalMaterial = overlayMaterial;
                overlayInstance.stopwatch = 0f;
                if (modelTransform.TryGetComponent(out CharacterModel overlayCharModel))
                    overlayInstance.AddToCharacterModel(overlayCharModel);
            }
        }
        public override void Update()
        {
            base.Update();

            if (overlayInstance != null)
            {
                overlayInstance.stopwatch = 0f;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (isAuthority && pyroController && pyroController.heat < pyroController.highHeat)
            {
                outer.SetNextState(new LowHeat());
            }
        }
    }
}
