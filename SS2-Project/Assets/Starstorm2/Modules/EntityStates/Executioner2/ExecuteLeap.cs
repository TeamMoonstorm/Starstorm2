using UnityEngine;
using RoR2;
using SS2.Components;
using UnityEngine.AddressableAssets;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace EntityStates.Executioner2
{
    public class ExecuteLeap : BaseSkillState
    {
        private Vector3 flyVector = Vector3.zero;
        private static float walkSpeedCoefficient = 2f;
        private static float baseVerticalSpeed = 48f;
        public static AnimationCurve speedCoefficientCurve;
        private static float baseDuration = 0.8f;
        private static float maxAttackSpeed = 1.4f;
        private static float dumbFuckingSpeedScalingNumberCoefficientValue = 1.3f;
        public static float crosshairDur = 0.75f;

        private static float maxAngle = 42f;
        private static float maxAngleTuah = 66f;
        private static float rayRadius = 1.5f;
        private static float axeFadeInDuration = .8f;

        public static GameObject indicatorPrefab;
        public static GameObject indicatorPrefabMastery;
        public static GameObject jumpEffect;
        public static GameObject jumpEffectMastery;

        public static string ExhaustL;
        public static string ExhaustR;
        private bool controlledExit = false;

        private float duration;
        private float verticalSpeed;
        private ExecutionerController exeController;
        private GameObject indicatorInstance;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private static float cameraLerpDuration = 0.5f;
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
                exeController.AxeFadeIn(axeFadeInDuration);
            }

            float attackSpeed = Mathf.Min(attackSpeedStat, maxAttackSpeed);
            duration = baseDuration / attackSpeed;
            float dipshit = ((attackSpeed - 1) * dumbFuckingSpeedScalingNumberCoefficientValue) + 1;
            verticalSpeed = baseVerticalSpeed * dipshit;

            characterBody.hideCrosshair = true;
            characterBody.SetAimTimer(duration);
            characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;
            flyVector = Vector3.up;

            Util.PlaySound("ExecutionerSpecialCast", gameObject);
            PlayAnimation("FullBody, Override", "SpecialJump", "Special.playbackRate", duration);
            StartAimMode();

            GameObject indicator = exeController.inMasterySkin ? indicatorPrefabMastery : indicatorPrefab;
            if (indicator)
            {
                indicatorInstance = GameObject.Instantiate(indicator);
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
                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, cameraLerpDuration);

                var packStateMachine = EntityStateMachine.FindByCustomName(gameObject, "JumpPack");
                if (packStateMachine)
                {
                    packStateMachine.SetNextStateToMain();
                }
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
                if (Util.CharacterSpherecast(gameObject, aimRay, rayRadius, out RaycastHit hit, 1000f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore))
                {
                    indicatorInstance.transform.position = hit.point;
                    indicatorInstance.transform.up = hit.normal;
                }
                else
                {
                    indicatorInstance.transform.position = aimRay.GetPoint(1000f);
                    indicatorInstance.transform.up = Vector3.up;
                }

                // prevent spherecast jank so we dont go up/sideways
                Vector3 between = indicatorInstance.transform.position - aimRay.origin;
                Vector3 directionTuah = Vector3.RotateTowards(Vector3.down, between.normalized, maxAngleTuah * Mathf.Deg2Rad, 0f);
                aimRay.direction = directionTuah;
                if(Util.CharacterRaycast(gameObject, aimRay, out RaycastHit hitTuah, 1000f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore))
                {
                    indicatorInstance.transform.position = hitTuah.point;
                    indicatorInstance.transform.up = hitTuah.normal;
                }
                else
                {
                    indicatorInstance.transform.position = aimRay.GetPoint(1000f);
                    indicatorInstance.transform.up = Vector3.up;
                }
            }
        }

        private void FixedUpdateAuthority()
        {
            if (fixedAge >= duration)
            {
                controlledExit = true;
                
                Vector3 direction = (indicatorInstance.transform.position - characterBody.footPosition).normalized;
                direction = Vector3.RotateTowards(Vector3.down, direction, maxAngleTuah * Mathf.Deg2Rad, 0f);
                var nextState = InstantiateNextState(direction.normalized);
                outer.SetNextState(nextState);
            }
            else
            {
                HandleMovement();
            }
        }

        public virtual EntityState InstantiateNextState(Vector3 dashVector)
        {
            return new ExecuteSlam { dashVector = dashVector };
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
            characterDirection.targetVector = inputBank.aimDirection;
            characterDirection.moveVector = Vector3.zero;
        }

        public override void OnExit()
        {
            base.OnExit();
            if(indicatorInstance)
            {
                Destroy(indicatorInstance);
            }
            if (outer.nextState is not ExecuteSlam)
            {
                PlayAnimation("FullBody, Override", "BufferEmpty");
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

    public class ExecuteLeapScepter : ExecuteLeap
    {
        public override EntityState InstantiateNextState(Vector3 dashVector)
        {
            return new ExecuteSlamScepter { dashVector = dashVector };
        }
    }
}

