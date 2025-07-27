using SS2;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Executioner2
{
    public class ChargeGun : BaseSkillState
    {
        public static float baseDuration = 1.2f;

        private static float camEntryDuration = 0.2f;
        private static float camExitDuration = 0.4f;

        private static string enterSoundString = "ExecutionerAimSecondary";
        private static string exitSoundString = "ExecutionerExitSecondary";

        [SerializeField]
        public SkillDef primaryOverride;

        private GenericSkill overriddenSkill;

        private bool thisFuckingSucks;

        private float duration;

        private bool useAltCamera = false;


        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData chargeCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 70f,
            minPitch = -70f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = chargeCameraPos,
            wallCushion = 0.1f,
        };

        private CharacterCameraParamsData altCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 70f,
            minPitch = -70f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = altCameraPos,
            wallCushion = 0.1f,
        };

        private static Vector3 chargeCameraPos = new Vector3(1.2f, -0.65f, -6.1f);

        private static Vector3 altCameraPos = new Vector3(-1.2f, -0.65f, -6.1f);
        public override void OnEnter()
        {
            base.OnEnter();

            duration = (baseDuration * skillLocator.secondary.cooldownScale);

            Util.PlaySound(enterSoundString, gameObject);
            PlayCrossfade("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration, 0.3f);

            characterBody.SetAimTimer(2f);

            CameraSwap();

            GenericSkill primarySkill = skillLocator.primary;
            if (primarySkill)
            {
                if (!overriddenSkill)
                {
                    overriddenSkill = primarySkill;
                    overriddenSkill.SetSkillOverride(primarySkill, primaryOverride, GenericSkill.SkillOverridePriority.Replacement);
                }
            }
        }

        private void CameraSwap()
        {
            if (useAltCamera)
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

        public override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.V) && isAuthority)
            {
                useAltCamera = !useAltCamera;
                CameraSwap();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.isSprinting = false;
            characterBody.aimTimer = 2f;

            if (!thisFuckingSucks && skillLocator.secondary.stock == 0)
            {
                thisFuckingSucks = true;
                PlayCrossfade("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration, 0.3f);
            }

            if (thisFuckingSucks && skillLocator.secondary.stock >= 1)
            {
                thisFuckingSucks = false;
            }

            if (isAuthority)
            {
                if (!inputBank.skill2.down || inputBank.skill3.down || inputBank.skill4.down)
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
            PlayCrossfade("Gesture, Override", "BufferEmpty", "Secondary.playbackRate", duration, 0.3f);
            Util.PlaySound(exitSoundString, gameObject);
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, camExitDuration);
            }
            if (overriddenSkill)
            {
                overriddenSkill.UnsetSkillOverride(skillLocator.primary, primaryOverride, GenericSkill.SkillOverridePriority.Replacement);
            }
            skillLocator.primary.RemoveAllStocks();
        }
    }
}