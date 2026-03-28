using RoR2;
using UnityEngine;
namespace SS2.Components
{
    public class BloodTesterAnimator : MonoBehaviour
    {
        public ParticleSystem healEffectSystem;
        public ParticleSystem stageUpEffectSystem;
        private static float materialInDuration = 0.2f;
        private static float materialOutDuration = 0.2f;
        private int currentStage;
        private CharacterModel characterModel;
        private TemporaryOverlayInstance overlayIn;
        private TemporaryOverlayInstance overlayOut;
        private bool overlayActive = false;
        private void Start()
        {
            this.characterModel = base.GetComponentInParent<CharacterModel>();
        }

        private void FixedUpdate()
        {
            CharacterBody body = this.characterModel?.body;
            if (body)
            {
                SetStage(body.GetBuffCount(SS2Content.Buffs.BuffBloodTesterRegen));
            }
        }
        private void SetStage(int i)
        {
            if (currentStage == i) return;
            if (i > currentStage)
            {
                stageUpEffectSystem.Play();
                if(Util.HasEffectiveAuthority(this.characterModel.body.gameObject))
                    Util.PlaySound("ActivateBloodTester", RoR2Application.instance.gameObject);
            }
            currentStage = i;
            ParticleSystem.EmissionModule emission = healEffectSystem.emission;

            if (currentStage == 0)
            {
                if (overlayActive)
                {
                    overlayActive = false;
                    EndOverlay();
                }
                emission.rateOverTimeMultiplier = 0;
            }
            else if (currentStage > 0)
            {
                if (!overlayActive)
                {
                    overlayActive = true;
                    StartOverlay();
                }
                emission.rateOverTimeMultiplier = 10 * currentStage;
            }
        }

        private void StartOverlay()
        {
            if (overlayOut != null)
            {
                overlayOut.RemoveFromCharacterModel();
            }

            // Initialize overlayIn. shouldnt need to do this more than once
            if (overlayIn == null)
            {
                overlayIn = TemporaryOverlayManager.AddOverlay(characterModel.gameObject);
                overlayIn.duration = materialInDuration;
                overlayIn.animateShaderAlpha = true;
                overlayIn.alphaCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
                overlayIn.destroyComponentOnEnd = false;
                overlayIn.originalMaterial = SS2Assets.LoadAsset<Material>("matHealOverlayBright", SS2Bundle.Items);
            }

            overlayIn.AddToCharacterModel(characterModel);
        }

        private void EndOverlay()
        {
            if (overlayIn != null)
            {
                overlayIn.RemoveFromCharacterModel();
            }

            // Initialize overlayOut. shouldnt need to do this more than once
            if (overlayOut == null)
            {
                overlayOut = TemporaryOverlayManager.AddOverlay(characterModel.gameObject);
                overlayOut.duration = materialOutDuration;
                overlayOut.animateShaderAlpha = true;
                overlayOut.alphaCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
                overlayOut.destroyComponentOnEnd = false;
                overlayOut.originalMaterial = SS2Assets.LoadAsset<Material>("matHealOverlayBright", SS2Bundle.Items);
            }

            overlayOut.AddToCharacterModel(characterModel);
        }

        
    }
}
