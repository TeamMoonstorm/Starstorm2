using Moonstorm;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


// Graph of Executioner's slam 
// Outdated
// https://www.desmos.com/calculator/dx5i257vbx

namespace EntityStates.Executioner
{
    public class AxeSlam : BaseSkillState
    {
        [TokenModifier("SS2_EXECUTIONER_AXE_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float baseDamageCoefficient = 12f;
        ///<summary>What percentage of baseDamageCoefficient should be made up of force damage. Put a value between 0 and 1 you ape.</summary>
        public static float forceDamageCoefficient = 0.5f;
        public static float procCoefficient = 1.0f;
        public static float maxDuration = 10f;
        public static float upwardDuration = 0.9f;
        ///<summary>Duration between when he starts going up and starts going down</summary>
        public static float slamRadius = 14f;
        public static float recoil = 8f;
        public static float rechargePerKill = 1.0f;
        public static float downwardAcceleration = -125f;
        public static AnimationCurveAsset upwardSpeedCoefficientCurve;
        public static GameObject slamEffect;
        public static GameObject axeEffect;

        //This must be recalculated if you fuck with this at all. It's just his velocity when he hits the ground when he doesn't move positions.
        private const float standardDownwardVelocity = -302.2f;

        private Animator animator;
        private Vector3 flyVector = Vector3.up;
        private AnimateShaderAlpha[] axeShaderAnimators = Array.Empty<AnimateShaderAlpha>();

        private bool hasDoneIntro;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData slamCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 70f,
            minPitch = -70f,
            pivotVerticalOffset = 1f, //how far up should the camera go?
            idealLocalCameraPos = slamCameraPosition,
            wallCushion = 0.1f
        };
        public static Vector3 slamCameraPosition = new Vector3(0f, 0f, -17.5f); // how far back should the camera go?

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            characterMotor.onHitGroundAuthority += GroundSlam;
            Util.PlaySound("ExecutionerSpecialCast", gameObject);
            PlayAnimation("FullBody, Override", "Special1", "Special.playbackRate", upwardDuration);
            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 0.5f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matHuntressFlashBright");
                temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }
            Transform axeSpawn = FindModelChild("AxeSpawn");
            if (axeSpawn)
            {
                //This only works if these exist
                var axeEffectInstance = UnityEngine.Object.Instantiate(axeEffect, axeSpawn, false);
                axeShaderAnimators = axeEffectInstance.GetComponents<AnimateShaderAlpha>();
                foreach (var shaderAlpha in axeShaderAnimators)
                    shaderAlpha.timeMax = upwardDuration;
            }
            if (NetworkServer.active)
                characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
            if (isAuthority)
            {
                characterMotor.Motor.ForceUnground();

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = slamCameraParams,
                    priority = 0f
                };
                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.15f);
                //aimRequest = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
        }

        public override void Update()
        {
            base.Update();
            if (age > upwardDuration && !hasDoneIntro)
            {
                PlayAnimation("FullBody, Override", "Special2");
                hasDoneIntro = true;
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
            if (fixedAge >= maxDuration)
                outer.SetNextStateToMain();
            else
                HandleMovement();
        }

        public void HandleMovement()
        {
            float moveSpeed = Mathf.Clamp(0f, 11f, 0.5f * moveSpeedStat);
            if (fixedAge < upwardDuration)
            {
                characterMotor.rootMotion += flyVector * (moveSpeed * upwardSpeedCoefficientCurve.value.Evaluate(fixedAge / upwardDuration) * Time.fixedDeltaTime);
                characterMotor.velocity.y = 0f;
            }
            else
            {
                characterMotor.velocity += flyVector * (moveSpeedStat * downwardAcceleration * Time.fixedDeltaTime);
            }
        }

        private void GroundSlam(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            //get number of enemies hit to divide damage
            //LogCore.LogI($"Velocity {hitGroundInfo.velocity}");

            SphereSearch search = new SphereSearch();
            List<HurtBox> hits = new List<HurtBox>();
            List<HealthComponent> hitTargets = new List<HealthComponent>();

            float damage = baseDamageCoefficient - (baseDamageCoefficient * forceDamageCoefficient);
            float forceDamage = baseDamageCoefficient * forceDamageCoefficient * Mathf.Clamp(hitGroundInfo.velocity.y / standardDownwardVelocity, 0f, 5f);
            damage += forceDamage;

            search.ClearCandidates();
            search.origin = hitGroundInfo.position;
            search.radius = slamRadius;
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamComponent.teamIndex));
            search.GetHurtBoxes(hits);
            hitTargets.Clear();
            foreach (HurtBox h in hits)
            {
                HealthComponent hp = h.healthComponent;
                if (hp && !hitTargets.Contains(hp))
                    hitTargets.Add(hp);
            }
            if (hitTargets.Count <= 1)
                damage *= 2f;

            bool crit = RollCrit();
            BlastAttack blast = new BlastAttack()
            {
                radius = slamRadius,
                procCoefficient = procCoefficient,
                position = hitGroundInfo.position,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = characterBody.damage * damage,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.BypassOneShotProtection
            };
            blast.Fire();

            AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);
            if (slamEffect)
                EffectManager.SimpleEffect(slamEffect, hitGroundInfo.position, Quaternion.identity, true);

            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayAnimation("FullBody, Override", "Special4", "Special.playbackRate", 0.4f);
            characterMotor.onHitGroundAuthority -= GroundSlam;
            characterBody.bodyFlags -= CharacterBody.BodyFlags.IgnoreFallDamage;
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, .4f);
                //cameraTargetParams.RemoveRequest(aimRequest);
            }
            if (NetworkServer.active)
                characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
            if (axeShaderAnimators.Length > 0)
                axeShaderAnimators[1].enabled = true;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}