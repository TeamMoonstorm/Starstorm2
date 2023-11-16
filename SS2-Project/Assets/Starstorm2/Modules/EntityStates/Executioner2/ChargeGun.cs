using Moonstorm.Starstorm2;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.Executioner2
{
    public class ChargeGun : BaseSkillState
    {
        public static float baseDuration = 1.2f;
        [HideInInspector]
        public static GameObject chargeEffectPrefab = null;
        public static GameObject defaultChargeEffectPrefab;
        public static GameObject masteryChargeEffectPrefab;
        private GameObject chargeEffectInstance;

        public static float timeBetweenStocks = 1.2f;

        [HideInInspector]
        public static GameObject plumeEffect = SS2Assets.LoadAsset<GameObject>("exePlume", SS2Bundle.Executioner2);
        [HideInInspector]
        public static GameObject plumeEffectLarge = SS2Assets.LoadAsset<GameObject>("exePlumeBig", SS2Bundle.Executioner2);
        public static GameObject defaultPlume;
        public static GameObject defaultPlumeLarge;
        //public static GameObject masteryPlume;
        //public static GameObject masteryPlumeLarge;

        [SerializeField]
        public SkillDef primaryOverride;
        private SkillDef overrideDef;

        private GenericSkill overriddenSkill;

        private bool thisFuckingSucks;

        private float chargeTimer = 0f;
        private float duration;

        private bool isPressingCameraSwap;
        private bool useAltCamera = false;

        private NetworkStateMachine nsm;
        private EntityStateMachine weaponEsm;

        private string skinNameToken;

        private float timer;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
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

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            if (skinNameToken == "SS2_SKIN_EXECUTIONER_MASTERY")
            {
                chargeEffectPrefab = masteryChargeEffectPrefab;
                //plumeEffect = masteryPlume;
                //plumeEffectLarge = masteryPlumeLarge;
            }
            else
            {
                chargeEffectPrefab = defaultChargeEffectPrefab;
                //plumeEffect = defaultPlume;
                //plumeEffectLarge = defaultPlumeLarge;
            }

            nsm = GetComponent<NetworkStateMachine>();
            if (nsm != null)
                weaponEsm = nsm.stateMachines[1];

            if (inputBank.skill4.down)
            {
                outer.SetNextStateToMain();
                return;
            }

            duration = (baseDuration * skillLocator.secondary.cooldownScale);

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

            if (!thisFuckingSucks && skillLocator.secondary.stock == 0)
            {
                thisFuckingSucks = true;
                PlayCrossfade("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration, 0.3f);
            }

            if (thisFuckingSucks && skillLocator.secondary.stock >= 1)
            {
                thisFuckingSucks = false;
            }

            /*if (fixedAge >= duration && inputBank.skill2.down)
            {
                ChargeGun nextState = new ChargeGun();
                nextState.useAltCamera = useAltCamera;
                outer.SetNextState(nextState);
                return;
            }*/

            if (isAuthority)
            {
                if (!inputBank.skill2.down || inputBank.skill3.down || inputBank.skill4.down)
                {
                    outer.SetNextStateToMain();
                    return;
                }
            }

            if (weaponEsm.IsInMainState())
                timer += Time.fixedDeltaTime;
            else
                timer = 0f;

            if (timer >= (timeBetweenStocks * skillLocator.secondary.cooldownScale) / attackSpeedStat && characterBody.skillLocator.secondary.stock < characterBody.skillLocator.secondary.maxStock)
            {
                timer = 0f;

                skillLocator.secondary.stock += 1;
                //Debug.Log("adding stock");

                if (characterBody.skillLocator.secondary.stock < characterBody.skillLocator.secondary.maxStock)
                {
                    Util.PlaySound("ExecutionerGainCharge", gameObject);
                    EffectManager.SimpleMuzzleFlash(plumeEffect, gameObject, "ExhaustL", true);
                    EffectManager.SimpleMuzzleFlash(plumeEffect, gameObject, "ExhaustR", true);
                }
                if (characterBody.skillLocator.secondary.stock >= characterBody.skillLocator.secondary.maxStock)
                {
                    Util.PlaySound("ExecutionerMaxCharge", gameObject);
                    EffectManager.SimpleMuzzleFlash(plumeEffectLarge, gameObject, "ExhaustL", true);
                    EffectManager.SimpleMuzzleFlash(plumeEffectLarge, gameObject, "ExhaustR", true);
                    EffectManager.SimpleEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/LightningFlash.prefab").WaitForCompletion(), characterBody.corePosition, Quaternion.identity, false);
                }

                characterBody.SetAimTimer(timeBetweenStocks);
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
                if (skinNameToken == "SS2_SKIN_EXECUTIONER_MASTERY")
                    chargeEffectInstance = UnityEngine.Object.Instantiate(masteryChargeEffectPrefab, muzzle.position, muzzle.rotation);
                else
                    chargeEffectInstance = UnityEngine.Object.Instantiate(defaultChargeEffectPrefab, muzzle.position, muzzle.rotation);
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
            skillLocator.primary.RemoveAllStocks();
        }
    }
}