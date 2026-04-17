using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Duke
{
    public class AimDownSights : BaseSkillState
    {
        private static float camEntryDuration = 0.2f;
        private static float camExitDuration = 0.4f;
        private static float overrideDelay = 0.175f;

        private static string enterSoundString = "Play_bandit2_m2_charge";
        private static string exitSoundString = "Play_bandit2_m2_uncharge";

        public static GameObject crosshairOverridePrefab;

        [SerializeField]
        public SkillDef adsFireSkillDef;

        private float overrideStopwatch;
        private bool overrideSet;
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;
        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private SS2.Components.DukeController dukeController;

        private CharacterCameraParamsData adsCameraParams = new CharacterCameraParamsData
        {
            pivotVerticalOffset = 1.37f,
            idealLocalCameraPos = new Vector3(1.2f, -0.75f, -6.1f),
        };

        private CharacterCameraParamsData altCameraParams = new CharacterCameraParamsData
        {
            pivotVerticalOffset = 1.37f,
            idealLocalCameraPos = new Vector3(-1.2f, -0.75f, -6.1f),
        };

        public override void OnEnter()
        {
            base.OnEnter();

            Util.PlaySound(enterSoundString, gameObject);
            PlayCrossfade("Gesture, Override", "AimDownSights", "Secondary.playbackRate", 0.5f, 0.3f);

            characterBody.SetAimTimer(2f);

            if (gameObject.TryGetComponent(out SS2.Components.DukeController controller))
            {
                dukeController = controller;
            }
            else
            {
                Debug.LogError("[Duke] AimDownSights: Failed to get DukeController component.");
            }

            ApplyCameraOverride();

            if (skillLocator.secondary.stock > 0)
            {
                TrySetSkillOverride();
            }

            if (crosshairOverridePrefab)
            {
                crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
            }
        }

        private void ApplyCameraOverride()
        {
            bool useAlt = dukeController && dukeController.useAltCamera;

            cameraTargetParams.RemoveParamsOverride(camOverrideHandle, camExitDuration);

            var request = new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = useAlt ? altCameraParams : adsCameraParams,
                priority = 0f
            };
            camOverrideHandle = cameraTargetParams.AddParamsOverride(request, camEntryDuration);
        }

        private void TrySetSkillOverride()
        {
            overrideStopwatch += Time.fixedDeltaTime;
            if (!overrideSet && overrideStopwatch >= overrideDelay)
            {
                overrideSet = true;
                skillLocator.primary.SetSkillOverride(this, adsFireSkillDef, GenericSkill.SkillOverridePriority.Replacement);
            }
        }

        private void UnsetSkillOverride()
        {
            overrideStopwatch = 0f;
            if (overrideSet)
            {
                overrideSet = false;
                skillLocator.primary.UnsetSkillOverride(this, adsFireSkillDef, GenericSkill.SkillOverridePriority.Replacement);
            }
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.V) && isAuthority && dukeController)
            {
                dukeController.useAltCamera = !dukeController.useAltCamera;
                ApplyCameraOverride();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.isSprinting = false;
            characterBody.SetAimTimer(2f);

            if (skillLocator.secondary.stock > 0)
            {
                TrySetSkillOverride();
            }
            else
            {
                UnsetSkillOverride();
            }

            if (isAuthority && !inputBank.skill2.down)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            UnsetSkillOverride();
            PlayCrossfade("Gesture, Override", "BufferEmpty", "Secondary.playbackRate", 0.3f, 0.3f);
            Util.PlaySound(exitSoundString, gameObject);

            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, camExitDuration);
            }

            if (crosshairOverrideRequest != null)
            {
                crosshairOverrideRequest.Dispose();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
