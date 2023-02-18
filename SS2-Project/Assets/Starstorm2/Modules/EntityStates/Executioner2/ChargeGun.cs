using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.Executioner2
{
    public class ChargeGun : BaseSkillState
    {
        public static float baseDuration = 1.2f;
        public static GameObject chargeEffectPrefab;
        private GameObject chargeEffectInstance;

        [SerializeField]
        public SkillDef primaryOverride;
        private SkillDef overrideDef;

        private GenericSkill overriddenSkill;

        private bool thisFuckingSucks;

        private float chargeTimer = 0f;
        private float duration;

        private bool isPressingCameraSwap;
        private bool useAltCamera = false;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData chargeCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 85f,
            minPitch = -85f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = chargeCameraPos,
            wallCushion = 0.1f,
        };

        private CharacterCameraParamsData altCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 85f,
            minPitch = -85f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = altCameraPos,
            wallCushion = 0.1f,
        };

        [HideInInspector]
        public static Vector3 chargeCameraPos = new Vector3(1.2f, -0.65f, -6.1f);

        [HideInInspector]
        public static Vector3 altCameraPos = new Vector3(-1.2f, -0.65f, -6.1f);
        public override void OnEnter()
        {
            base.OnEnter();
            //Debug.Log("entering");
            duration = baseDuration / skillLocator.secondary.cooldownScale;
            characterBody.SetBuffCount(Moonstorm.Starstorm2.SS2Content.Buffs.bdExeCharge.buffIndex, 1);

            PlayCrossfade("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration, 0.3f);

            characterBody.SetAimTimer(2f);

            CameraSwap();

            //GenericSkill manipulatorSkill = skillLocator.FindSkillByFamilyName("sfExe2Manipulators");

            Debug.Log(skillLocator.FindSkill("exeManipulators").skillDef.skillName);

            //overrideDef = manipulatorSkill.skillDef;

            //Debug.Log(overrideDef);

            GenericSkill primarySkill = skillLocator.primary;

            Debug.Log(primarySkill);
            Debug.Log(primaryOverride);

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
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.2f);

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = altCameraParams,
                    priority = 0f
                };

                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.2f);
            }
            else
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.2f);

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = chargeCameraParams,
                    priority = 0f
                };

                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.2f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!thisFuckingSucks && skillLocator.secondary.stock == 0)
            {
                thisFuckingSucks = true;
                PlayCrossfade("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration, 0.3f);
            }

            if (thisFuckingSucks && skillLocator.secondary.stock >= 1)
            {
                thisFuckingSucks = false;
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                useAltCamera = !useAltCamera;
                CameraSwap();
            }

            /*if (fixedAge >= duration && inputBank.skill2.down)
            {
                ChargeGun nextState = new ChargeGun();
                nextState.useAltCamera = useAltCamera;
                outer.SetNextState(nextState);
                return;
            }*/   
            
            if (!inputBank.skill2.down)
            {
                outer.SetNextStateToMain();
                return;
            }

            /*if (inputBank.skill1.down && skillLocator.secondary.stock >= 1)
            {
                characterBody.SetBuffCount(Moonstorm.Starstorm2.SS2Content.Buffs.bdExeCharge.buffIndex, 0);
                PlayAnimation("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration);
                FireChargeGun nextState = new FireChargeGun();
                nextState.activatorSkillSlot = activatorSkillSlot;
                if (skillLocator.secondary.stock >= skillLocator.secondary.maxStock)
                    nextState.fullBurst = true;
                outer.SetNextState(nextState);
            }*/

            if (chargeEffectPrefab && !chargeEffectInstance && fixedAge >= 0.3f)
            {
                ChildLocator cl = modelLocator.modelTransform.GetComponent<ChildLocator>();
                Transform muzzle = cl.FindChild("ExhaustGun");
                chargeEffectInstance = Object.Instantiate(chargeEffectPrefab, muzzle.position, muzzle.rotation);
                chargeEffectInstance.transform.parent = muzzle.transform;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            base.OnExit();
            characterBody.SetBuffCount(Moonstorm.Starstorm2.SS2Content.Buffs.bdExeCharge.buffIndex, 0);
            PlayCrossfade("Gesture, Override", "BufferEmpty", "Secondary.playbackRate", duration, 0.3f);
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.7f);
            }
            if (overriddenSkill)
            {
                overriddenSkill.UnsetSkillOverride(skillLocator.primary, primaryOverride, GenericSkill.SkillOverridePriority.Replacement);
            }
            if (chargeEffectInstance)
                Destroy(chargeEffectInstance);
        }
    }

}