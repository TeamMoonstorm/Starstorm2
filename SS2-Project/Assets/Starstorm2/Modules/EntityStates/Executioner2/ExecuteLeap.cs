using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;
using UnityEngine.AddressableAssets;

namespace EntityStates.Executioner2
{
    public class ExecuteLeap : BaseSkillState
    {
        private Vector3 flyVector = Vector3.zero;
        public static AnimationCurve speedCoefficientCurve;
        public static float duration = 0.8f;

        public static GameObject jumpEffect;
        public static GameObject jumpEffectMastery;
        public static Material jumpMaterialMastery;
        public static string ExhaustL;
        public static string ExhaustR;

        private string skinNameToken;

        private ExecutionerController exeController;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData slamCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 88f,
            minPitch = 25f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = slamCameraPosition,
            wallCushion = 0.1f,
        };
        public static Vector3 slamCameraPosition = new Vector3(2.6f, -2.0f, -4f);
        public override void OnEnter()
        {
            base.OnEnter();

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            exeController = GetComponent<ExecutionerController>();
            if (exeController != null)
                exeController.meshExeAxe.SetActive(true);

            //skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            characterBody.hideCrosshair = true;
            characterBody.SetAimTimer(duration);

            flyVector = Vector3.up;

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();


                temporaryOverlay.animateShaderAlpha = true;

                temporaryOverlay.destroyComponentOnEnd = true;

                if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
                {
                    temporaryOverlay.duration = .3f * duration;
                    temporaryOverlay.originalMaterial = jumpMaterialMastery;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0.5f, 0.5f, 100f);
                }
                else
                {
                    temporaryOverlay.duration = 1.5f * duration;
                    temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlashBright.mat").WaitForCompletion();
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0.5f, 0.5f, 0f);
                }

                temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }

            Util.PlaySound("ExecutionerSpecialCast", gameObject);
            PlayAnimation("FullBody, Override", "SpecialJump", "Special.playbackRate", duration);

            if (isAuthority)
            {
                characterMotor.Motor.ForceUnground();

                if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
                {
                    EffectManager.SimpleMuzzleFlash(jumpEffectMastery, gameObject, ExhaustL, true);
                    EffectManager.SimpleMuzzleFlash(jumpEffectMastery, gameObject, ExhaustR, true);
                }
                else
                {
                    EffectManager.SimpleMuzzleFlash(jumpEffect, gameObject, ExhaustL, true);
                    EffectManager.SimpleMuzzleFlash(jumpEffect, gameObject, ExhaustR, true);
                }

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = slamCameraParams,
                    priority = 1f
                };
                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.5f);
            }    
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
                FixedUpdateAuthority();
        }

        private void FixedUpdateAuthority()
        {
            if (fixedAge >= duration)
            {
                if (inputBank.skill4.down)
                {
                    ExecuteHold nextState = new ExecuteHold();
                    outer.SetNextState(nextState);
                }
                else
                {
                    ExecuteSlam nextState = new ExecuteSlam();
                    outer.SetNextState(nextState);
                }
            }
            else
                HandleMovement();
        }

        public void HandleMovement()
        {
            characterMotor.rootMotion += flyVector * (moveSpeedStat * speedCoefficientCurve.Evaluate(fixedAge / duration) * Time.fixedDeltaTime * 2f);
            characterMotor.velocity.y = 0f;
        }

        public override void OnExit()
        {
            base.OnExit();
            characterBody.hideCrosshair = false;
            if (exeController != null)
                exeController.meshExeAxe.SetActive(false);
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, .1f);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}

