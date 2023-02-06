using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Executioner2
{
    public class ExecuteLeap : BaseSkillState
    {
        private Vector3 flyVector = Vector3.zero;
        public static AnimationCurve speedCoefficientCurve;
        public static float duration = 0.8f;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData slamCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 88f,
            minPitch = 55f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = slamCameraPosition,
            wallCushion = 0.1f,
        };
        public static Vector3 slamCameraPosition = new Vector3(1.6f, 0.0f, -9f);
        public override void OnEnter()
        {
            base.OnEnter();

            flyVector = Vector3.up;

            PlayAnimation("FullBody, Override", "SpecialJump", "Special.playbackRate", duration);

            if (isAuthority)
            {
                characterMotor.Motor.ForceUnground();

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
                ExecuteHold nextState = new ExecuteHold();
                outer.SetNextState(nextState);
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
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, .1f);
            }
        }
    }
}

