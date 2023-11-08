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

namespace EntityStates.Executioner2
{
    public class ExecuteHold : BaseSkillState
    {
        public static float duration = 1f;

        public static GameObject jumpEffect;
        public static GameObject jumpEffectMastery;
        public static string ExhaustL;
        public static string ExhaustR;
        private string skinNameToken;

        public static GameObject areaIndicator;
        public static GameObject areaIndicatorOOB;

        [HideInInspector]
        public GameObject areaIndicatorInstance;

        [HideInInspector]
        public GameObject areaIndicatorInstanceOOB;

        private ExecutionerController exeController;
        private bool controlledExit = false;

        public bool imAFilthyFuckingLiar = false;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData slamCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 88f,
            minPitch = 25f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = slamCameraPosition,
            wallCushion = 0.1f,
        };

        [HideInInspector]
        public static Vector3 slamCameraPosition = new Vector3(2.6f, -2.0f, -8f);

        public override void OnEnter()
        {
            base.OnEnter();

            exeController = GetComponent<ExecutionerController>();
            if (exeController != null)
                exeController.meshExeAxe.SetActive(true);

            characterBody.hideCrosshair = true;

            PlayAnimation("FullBody, Override", "SpecialHang", "Special.playbackRate", duration);

            if (isAuthority)
            {
                skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

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
                    priority = 0f
                };

                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0f);

                characterBody.SetAimTimer(duration);

                areaIndicatorInstance = UnityEngine.Object.Instantiate(areaIndicator);
                //areaIndicatorInstanceOOB = UnityEngine.Object.Instantiate(areaIndicatorOOB);
                areaIndicatorInstance.SetActive(true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
                FixedUpdateAuthority();
        }

        public override void Update()
        {
            base.Update();
            UpdateAreaIndicator();
        }

        private void UpdateAreaIndicator()
        {
            if (areaIndicatorInstance)
            {
                float maxDistance = 256f;

                Ray aimRay = GetAimRay();
                RaycastHit raycastHit;
                if (Physics.Raycast(aimRay, out raycastHit, maxDistance, LayerIndex.CommonMasks.bullet))
                {
                    imAFilthyFuckingLiar = true;
                    //areaIndicatorInstance.SetActive(true);
                    //areaIndicatorInstanceOOB.SetActive(false);
                    areaIndicatorInstance.transform.position = raycastHit.point;
                    areaIndicatorInstance.transform.up = raycastHit.normal;
                }
                else
                {
                    imAFilthyFuckingLiar = false;
                    //areaIndicatorInstance.SetActive(false);
                    //areaIndicatorInstanceOOB.SetActive(true);
                    areaIndicatorInstance.transform.position = aimRay.GetPoint(maxDistance);
                    areaIndicatorInstance.transform.up = -aimRay.direction;
                }
            }
        }

        private void FixedUpdateAuthority()
        {
            if (fixedAge >= duration || !inputBank.skill4.down || inputBank.skill1.down)
            {
                ExecuteSlam nextState = new ExecuteSlam();
                nextState.wasLiedTo = imAFilthyFuckingLiar; //probably every time LOL 
                outer.SetNextState(nextState);
            }
            else
                HandleMovement();
        }

        public void HandleMovement()
        {
            characterMotor.velocity = Vector3.zero;
        }

        public override void OnExit()
        {
            base.OnExit();

            characterBody.hideCrosshair = false;

            if (exeController != null && controlledExit == false)
                exeController.meshExeAxe.SetActive(false);

            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 1f);
            }

            if (areaIndicatorInstance)
            {
                Destroy(areaIndicatorInstance.gameObject);
            }

            if (areaIndicatorInstanceOOB)
            {
                Destroy(areaIndicatorInstanceOOB.gameObject);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}

