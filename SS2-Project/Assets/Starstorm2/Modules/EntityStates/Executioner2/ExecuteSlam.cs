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
    public class ExecuteSlam : BaseSkillState
    {
        public static float baseDamageCoefficient = 12f;
        public static float slamRadius = 14f;
        public static float procCoefficient = 1.0f;
        public static float recoil = 8f;
        public static GameObject slamEffect;

        public static float duration = 1f;
        private bool hasSlammed = false;
        private Vector3 dashVector = Vector3.zero;

        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData slamCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 88f,
            minPitch = -70f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = slamCameraPosition,
            wallCushion = 0.1f,
        };
        public static Vector3 slamCameraPosition = new Vector3(0f, 0.0f, -32.5f);

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("FullBody, Override", "SpecialSwing", "Special.playbackRate", duration * 0.8f);

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            characterMotor.onHitGroundAuthority += GroundSlam;

            if (isAuthority)
            {
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
            characterDirection.forward = dashVector;
            if (!hasSlammed)
            {
                hasSlammed = true;
                dashVector = inputBank.aimDirection;
            }
            if (fixedAge >= duration)
                outer.SetNextStateToMain();
            else
                HandleMovement();
        }

        public void HandleMovement()
        {
            characterMotor.rootMotion += dashVector * moveSpeedStat * 15f * Time.fixedDeltaTime;
        }

        private void GroundSlam(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            //get number of enemies hit to divide damage
            //LogCore.LogI($"Velocity {hitGroundInfo.velocity}");

            SphereSearch search = new SphereSearch();
            List<HurtBox> hits = new List<HurtBox>();
            List<HealthComponent> hitTargets = new List<HealthComponent>();

            float damage = baseDamageCoefficient;

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

            PlayAnimation("FullBody, Override", "SpecialImpact", "Special.playbackRate", duration);

            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            characterMotor.onHitGroundAuthority -= GroundSlam;
            characterBody.bodyFlags -= CharacterBody.BodyFlags.IgnoreFallDamage;
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 1f);
            }
        }
    }
}

