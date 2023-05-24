using Moonstorm.Starstorm2.Orbs;
using Moonstorm.Starstorm2.Survivors;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using RoR2.UI;
using RoR2.HudOverlay;

namespace Moonstorm.Starstorm2.Components
{
    public class NemHuntressVisorController : MonoBehaviour
    {
        public CharacterModel characterModel;
        public SkillLocator skillLocator;
        public CharacterBody characterBody;
        public bool headsetLowered = false;
        public OverlayController overlayController;
        public GameObject scopeOverlayPrefab;

        public void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        public void Start()
        {
            skillLocator = characterBody.skillLocator;
        }

        public void FixedUpdate()
        {
            if (characterBody)
            {
                if (characterBody.outOfCombat)
                {
                    if (headsetLowered) RaiseHeadset();
                }
                else
                {
                    if (!headsetLowered) LowerHeadset();
                }
            }
        }

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        public void RaiseHeadset()
        {
            if (headsetLowered) headsetLowered = false;
            //Debug.Log("raising headset");
            if (overlayController != null)
            {
                HudOverlayManager.RemoveOverlay(overlayController);
                overlayController = null;
            }
        }

        public void LowerHeadset()
        {
            if (!headsetLowered) headsetLowered = true;
            //Debug.Log("lowering headset");
            overlayController = HudOverlayManager.AddOverlay(gameObject, new OverlayCreationParams
            {
                prefab = scopeOverlayPrefab,
                childLocatorEntry = "ScopeContainer"
            });
        }
    }
}