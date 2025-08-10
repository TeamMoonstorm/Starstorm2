using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2;
using R2API;

namespace EntityStates.NemExecutioner
{
    public class ChargeReap : BaseSkillState
    {
        private static float baseDuration = 1.5f;
        private static float maxDashDistance = 45;
        private static float minDashDistance = 10f;
        public static GameObject effectPrefab;
        public static GameObject indicatorPrefab;
        private static Vector3 indicatorScale = new Vector3(5f, 5f, 5f);
        private static string muzzle = "Chest";
        private static string chargeSoundString = "NemmandoDecisiveStrikeCharge";

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private static float cameraLerpDuration = 0.5f;
        private static float cameraMaxPitch = 88f;
        private static float cameraMinPitch = -88f;
        private static float cameraPivotVerticalOffset = 1.37f;
        private static float cameraFov = 70f;
        private static Vector3 cameraPosition = new Vector3(0f, 2.0f, -16f);
        private CharacterCameraParamsData cameraParams = new CharacterCameraParamsData
        {
            fov = cameraFov,
            maxPitch = cameraMaxPitch,
            minPitch = cameraMinPitch,
            pivotVerticalOffset = cameraPivotVerticalOffset,
            idealLocalCameraPos = cameraPosition,
            wallCushion = 0.1f,
        };

        private float duration;
        private uint soundID;
        private GameObject effectInstance;
        private GameObject indicatorInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            soundID = Util.PlayAttackSpeedSound(chargeSoundString, base.gameObject, attackSpeedStat);
            PlayAnimation("Gesture, Override", "ChargeReap", "Special.playbackRate", duration);
            characterBody.SetAimTimer(duration + 1f);
            Transform muzzleTransform = FindModelChild(muzzle) ?? characterBody.coreTransform;
            if (muzzleTransform && effectPrefab)
            {
                effectInstance = UnityEngine.Object.Instantiate<GameObject>(effectPrefab, muzzleTransform.position, muzzleTransform.rotation);
                effectInstance.transform.parent = muzzleTransform;
                ScaleParticleSystemDuration component = effectInstance.GetComponent<ScaleParticleSystemDuration>();
                ObjectScaleCurve component2 = effectInstance.GetComponent<ObjectScaleCurve>();
                if (component)
                {
                    component.newDuration = duration;
                }
                if (component2)
                {
                    component2.timeMax = duration;
                }
            }
            Transform modelTransform = GetModelTransform();
            if(modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = duration;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matNemergize", SS2Bundle.NemMercenary); //////////////////
            }
            if (cameraTargetParams)
            {
                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = cameraParams,
                    priority = 1f
                };
                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, cameraLerpDuration);
            }
            if(indicatorPrefab)
            {
                Ray aimRay = GetAimRay();
                indicatorInstance = GameObject.Instantiate(indicatorPrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction));
                indicatorInstance.transform.localScale = new Vector3(indicatorScale.x, indicatorScale.y, minDashDistance);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterMotor)
            {
                characterMotor.velocity = Vector3.zero;
                characterMotor.useGravity = false;
            }

            if (isAuthority && fixedAge > duration || !IsKeyDownAuthority())
            {
                float t = fixedAge / duration;
                float dashDistance = Mathf.Lerp(minDashDistance, maxDashDistance, t);
                outer.SetNextState(new Reap { dashDistance = dashDistance });
            }
        }

        public override void Update()
        {
            base.Update();
            if(indicatorInstance)
            {
                float t = age / duration;
                float dashDistance = Mathf.Lerp(minDashDistance, maxDashDistance, t);
                Ray aimRay = GetAimRay();
                indicatorInstance.transform.SetPositionAndRotation(aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction));
                indicatorInstance.transform.localScale = new Vector3(indicatorScale.x, indicatorScale.y, dashDistance);
            }
            
        }

        public override void OnExit()
        {
            if (characterMotor)
            {
                characterMotor.useGravity = true;
            }
            if (effectInstance)
            {
                Destroy(effectInstance);
            }
            if (indicatorInstance)
            {
                Destroy(indicatorInstance);
            }
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, .1f);
            }
            AkSoundEngine.StopPlayingID(soundID);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
    public class Reap : BaseState
    {
        public static GameObject dashPrefab;
        private static float smallHopVelocity;
        private static float dashDuration = 0.2f;
        private static string beginSoundString;
        private static string endSoundString;
        private static float damageCoefficient = 15f;
        private static float procCoefficient = 1;
        public static GameObject hitEffectPrefab;
        private static float hitPauseDuration = 0.3f;
        private float dashSpeed;

        private Transform modelTransform;
        private float stopwatch;
        private Vector3 dashVector = Vector3.zero;
        private OverlapAttack overlapAttack;
        private ChildLocator childLocator;
        private bool inHitPause;
        private float hitPauseTimer;


        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private static float cameraLerpDuration = 0.5f;
        private static float cameraMaxPitch = 88f;
        private static float cameraMinPitch = -88f;
        private static float cameraPivotVerticalOffset = 1.37f;
        private static float cameraFov = 90f;
        private static Vector3 cameraPosition = new Vector3(0, 1f, -12f);
        private CharacterCameraParamsData cameraParams = new CharacterCameraParamsData
        {
            fov = cameraFov,
            maxPitch = cameraMaxPitch,
            minPitch = cameraMinPitch,
            pivotVerticalOffset = cameraPivotVerticalOffset,
            idealLocalCameraPos = cameraPosition,
            wallCushion = 0.1f,
        };

        public float dashDistance;

        public override void OnEnter()
        {
            base.OnEnter();

            CreateDashEffect();
            PlayCrossfade("FullBody, Override", "AssaulterLoop", 0.1f);
            Util.PlaySound(Reap.beginSoundString, gameObject);
            modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay.duration = 1.2f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matNemergize", SS2Bundle.NemMercenary);
            }
            if (cameraTargetParams)
            {
                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = cameraParams,
                    priority = 2f
                };
                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, cameraLerpDuration);
            }

            dashVector = inputBank.aimDirection;
            characterDirection.forward = dashVector.normalized;
            dashSpeed = dashDistance / (Reap.dashDuration);

            gameObject.layer = LayerIndex.fakeActor.intVal;
            characterMotor.Motor.RebuildCollidableLayers();


            overlapAttack = InitMeleeOverlap(Reap.damageCoefficient, Reap.hitEffectPrefab, modelTransform, "Reap");
            overlapAttack.procCoefficient = procCoefficient;
            overlapAttack.damageType = DamageType.BonusToLowHealth;
            overlapAttack.damageType.AddModdedDamageType(SS2.Survivors.NemExecutioner.healNovaOnKill);
            overlapAttack.damageType.damageSource = DamageSource.Special;

            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
            }
        }

        public void CreateDashEffect()
        {
            Util.PlaySound("Play_nemmerc_dash", gameObject);
            childLocator = GetModelChildLocator();
            if(childLocator)
            {
                Transform transform = childLocator.FindChild("DashCenter");
                if (transform && Reap.dashPrefab)
                {
                    UnityEngine.Object.Instantiate<GameObject>(Reap.dashPrefab, transform.position, Util.QuaternionSafeLookRotation(dashVector), transform);
                }
            }
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterDirection.forward = dashVector;
            if (isAuthority)
            {
                characterMotor.velocity = Vector3.zero;
                if (!inHitPause)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (overlapAttack.Fire(null))
                    {
                        OnMeleeHitAuthority();
                    }
                    characterMotor.rootMotion += dashVector * dashSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    hitPauseTimer -= Time.fixedDeltaTime;
                    if (hitPauseTimer < 0f)
                    {
                        inHitPause = false;
                    }
                }
            }
            if (stopwatch >= dashDuration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void OnMeleeHitAuthority()
        {
            inHitPause = true;
            hitPauseTimer = Reap.hitPauseDuration / attackSpeedStat;

            if (modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay2 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                temporaryOverlay2.duration = Reap.hitPauseDuration / attackSpeedStat;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
            }
        }

        public override void OnExit()
        {
            gameObject.layer = LayerIndex.defaultLayer.intVal;
            characterMotor.Motor.RebuildCollidableLayers();
            Util.PlaySound(Reap.endSoundString, gameObject);
            if (isAuthority)
            {
                SmallHop(characterMotor, Reap.smallHopVelocity);
            }
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, .1f);
            }
            PlayAnimation("FullBody, Override", "EvisLoopExit", "Special.playbackRate", 1f);
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        
    }
}
