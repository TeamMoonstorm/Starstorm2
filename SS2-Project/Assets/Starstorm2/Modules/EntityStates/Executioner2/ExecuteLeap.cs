using UnityEngine;
using RoR2;
using SS2.Components;
using UnityEngine.AddressableAssets;

namespace EntityStates.Executioner2
{
    public class ExecuteLeap : BaseSkillState
    {
        private Vector3 flyVector = Vector3.zero;
        private static float walkSpeedCoefficient = 2f;
        private static float baseVerticalSpeed = 32f;
        public static AnimationCurve speedCoefficientCurve;
        private static float baseDuration = 0.8f;
        private static float maxAttackSpeed = 1.6f;
        private static float dumbFuckingSpeedScalingNumberCoefficientValue = 1.3f;
        public static float crosshairDur = 0.75f;

        private static float maxAngle = 42f;

        public static GameObject indicatorPrefab;
        public static GameObject jumpEffect;
        public static GameObject jumpEffectMastery;
        public static Material jumpMaterialMastery;
        public static string ExhaustL;
        public static string ExhaustR;
        private bool controlledExit = false;

        private float duration;
        private float verticalSpeed;
        private ExecutionerController exeController;
        private GameObject indicatorInstance;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData slamCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 88f,
            minPitch = -88f,
            pivotVerticalOffset = 1.37f,
            idealLocalCameraPos = slamCameraPosition,
            wallCushion = 0.1f,
        };
        private static Vector3 slamCameraPosition = new Vector3(2.6f, -2.0f, -12f);
        public override void OnEnter()
        {
            base.OnEnter();

            exeController = GetComponent<ExecutionerController>();
            if (exeController != null)
            {
                exeController.meshExeAxe.SetActive(true);
            }

            float attackSpeed = Mathf.Min(attackSpeedStat, maxAttackSpeed);
            duration = baseDuration / attackSpeed;
            float dipshit = ((attackSpeed - 1) * dumbFuckingSpeedScalingNumberCoefficientValue) + 1;
            verticalSpeed = baseVerticalSpeed * dipshit;

            characterBody.hideCrosshair = true;
            characterBody.SetAimTimer(duration);
            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;
            flyVector = Vector3.up;

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);

                temporaryOverlay.animateShaderAlpha = true;

                temporaryOverlay.destroyComponentOnEnd = true;

                if (exeController.inMasterySkin)
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

                // TODO: No longer needed post-SOTS, leaving in for now but need to remove later
                //temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }

            Util.PlaySound("ExecutionerSpecialCast", gameObject);
            PlayAnimation("FullBody, Override", "SpecialJump", "Special.playbackRate", duration);
            StartAimMode();

            if (indicatorPrefab)
            {
                indicatorInstance = GameObject.Instantiate(indicatorPrefab);
                indicatorInstance.transform.localScale = Vector3.one * ExecuteSlam.slamRadius;
                indicatorInstance.GetComponent<TeamFilter>().teamIndex = teamComponent.teamIndex;
                UpdateIndicator();
            }

            if (isAuthority)
            {
                characterMotor.Motor.ForceUnground();

                if (exeController.inMasterySkin)
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
            {
                FixedUpdateAuthority();
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateIndicator();
        }
        private void UpdateIndicator()
        {
            if (indicatorInstance)
            {
                Ray aimRay = base.GetAimRay();
                Vector3 direction = Vector3.down;
                direction = Vector3.RotateTowards(direction, aimRay.direction, maxAngle * Mathf.Deg2Rad, 0f);
                aimRay.direction = direction;
                if (Util.CharacterRaycast(gameObject, aimRay, out RaycastHit hit, 1000f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore))
                {
                    indicatorInstance.transform.position = hit.point;
                    indicatorInstance.transform.up = hit.normal;
                }
            }
        }

        private void FixedUpdateAuthority()
        {
            if (fixedAge >= duration)
            {
                controlledExit = true;
                ExecuteSlam nextState = new ExecuteSlam();
                outer.SetNextState(nextState);
            }
            else
            {
                HandleMovement();
            }
               
        }

        public void HandleMovement()
        {
            if(characterMotor.isGrounded)
            {
                characterMotor.Motor.ForceUnground();
            }

            float speed =  verticalSpeed * speedCoefficientCurve.Evaluate(fixedAge / duration);
            characterMotor.rootMotion += flyVector * speed  * Time.fixedDeltaTime;
            characterMotor.velocity.y = 0f;

            characterMotor.moveDirection = inputBank.moveVector;
            characterDirection.forward = inputBank.aimDirection;
        }

        public override void OnExit()
        {
            base.OnExit();
            if(indicatorInstance)
            {
                Destroy(indicatorInstance);
            }
            characterBody.hideCrosshair = false;
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            if (exeController != null && controlledExit == false)
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

