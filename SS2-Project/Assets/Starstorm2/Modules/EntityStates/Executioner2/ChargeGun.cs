using SS2.Components;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Executioner2
{
    public class ChargeGun : BaseSkillState
    {
        private static float baseDuration = 1.2f;
        private static float durationWithWhichYouWillNeedToHaveBeenInThisStateOrHadAtLeastOneStockToBeAbleToSetTheSkillOverride = 0.175f;
        private static float camEntryDuration = 0.2f;
        private static float camExitDuration = 0.4f;

        private static string enterSoundString = "ExecutionerAimSecondary";
        private static string exitSoundString = "ExecutionerExitSecondary";

        public static GameObject crosshairOverridePrefab;
        [SerializeField]
        public SkillDef primaryOverride;

        private bool thisFuckingSucks;

        private EntityStateMachine weapon;
        private float duration;

        private float overrideStopwatch;
        private ExecutionerController controller;
        private bool overrideSet;
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData chargeCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 70f,
            minPitch = -70f,
            pivotVerticalOffset = cameraPivotVerticalOffset,
            idealLocalCameraPos = chargeCameraPos,
            wallCushion = 0.1f,
        };

        private CharacterCameraParamsData altCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 70f,
            minPitch = -70f,
            pivotVerticalOffset = cameraPivotVerticalOffset,
            idealLocalCameraPos = altCameraPos,
            wallCushion = 0.1f,
        };

        private static float cameraPivotVerticalOffset = 1.37f;

        private static Vector3 chargeCameraPos = new Vector3(1.2f, -0.75f, -6.1f);

        private static Vector3 altCameraPos = new Vector3(-1.2f, -0.75f, -6.1f);
        public override void OnEnter()
        {
            base.OnEnter();

            duration = (baseDuration * skillLocator.secondary.cooldownScale);

            Util.PlaySound(enterSoundString, gameObject);
            PlayCrossfade("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration, 0.3f);

            characterBody.SetAimTimer(2f);

            controller = GetComponent<ExecutionerController>();
            CameraSwap();

            if (skillLocator.secondary.stock > 0)
            {
                SetSkillOverride();
            }

            if (crosshairOverridePrefab)
            {
                crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
            }

            weapon = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
        }

        private void CameraSwap()
        {
            if (controller && controller.useAltCamera)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, camExitDuration);

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = altCameraParams,
                    priority = 0f
                };

                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, camEntryDuration);
            }
            else
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, camExitDuration);

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = chargeCameraParams,
                    priority = 0f
                };

                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, camEntryDuration);
            }
        }

        private void SetSkillOverride()
        {
            overrideStopwatch += Time.fixedDeltaTime;
            if (!overrideSet && overrideStopwatch >= durationWithWhichYouWillNeedToHaveBeenInThisStateOrHadAtLeastOneStockToBeAbleToSetTheSkillOverride)
            {
                overrideSet = true;
                PlayCrossfade("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration, 0.3f);
                skillLocator.primary.SetSkillOverride(this, primaryOverride, GenericSkill.SkillOverridePriority.Replacement);
            }
            
        }

        private void UnsetSkillOverride()
        {
            overrideStopwatch = 0f;
            if (overrideSet)
            {
                overrideSet = false;
                skillLocator.primary.UnsetSkillOverride(this, primaryOverride, GenericSkill.SkillOverridePriority.Replacement);
            }
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.V) && isAuthority)
            {
                if (controller)
                {
                    controller.useAltCamera = !controller.useAltCamera;
                }
                
                CameraSwap();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (weapon.state is not Executioner2.Dash)
            {
                characterBody.isSprinting = false;
                characterBody.aimTimer = 2f;
            }
            else
            {
                characterBody.aimTimer = 0f;
            }

            if (skillLocator.secondary.stock > 0)
            {
                SetSkillOverride();
            }
            else
            {
                UnsetSkillOverride();
            }

            if (isAuthority)
            {
                if (!inputBank.skill2.down)
                {
                    outer.SetNextStateToMain();
                    return;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            base.OnExit();
            UnsetSkillOverride();
            PlayCrossfade("Gesture, Override", "BufferEmpty", "Secondary.playbackRate", duration, 0.3f);
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
    }
}