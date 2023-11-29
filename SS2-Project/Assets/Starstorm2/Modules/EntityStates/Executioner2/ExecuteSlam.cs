using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using RoR2;
using System;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;
using Moonstorm.Starstorm2;

namespace EntityStates.Executioner2
{
    public class ExecuteSlam : BaseSkillState
    {
        public static float baseDamageCoefficient;
        public static float slamRadius;
        public static float procCoefficient;
        public static float recoil;
        public static GameObject slamEffect;
        public static GameObject slamEffectMastery;
        private string skinNameToken;

        public static float duration = 1f;
        private bool hasSlammed = false;
        private Vector3 dashVector = Vector3.zero;

        private ExecutionerController exeController;

        public bool wasLiedTo = false;

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

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            exeController = GetComponent<ExecutionerController>();
            if (exeController != null)
            {
                exeController.meshExeAxe.SetActive(true);
                exeController.isExec = true;
            }
                

            characterBody.hideCrosshair = true;
            PlayAnimation("FullBody, Override", "SpecialSwing", "Special.playbackRate", duration * 0.8f);

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            characterMotor.onHitGroundAuthority += GroundSlam;

            characterBody.isSprinting = true;

            characterBody.SetAimTimer(duration);


            if (isAuthority)
            {
                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = slamCameraParams,
                    priority = 1f
                };
                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.1f);
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
            //if (exeController)
            //{
            //    if (exeController.hasOOB)
            //    {
            //        SS2Log.Info("Has OOBed");
            //    }
            //}

            characterDirection.forward = dashVector;
            if (!hasSlammed)
            {
                hasSlammed = true;
                dashVector = inputBank.aimDirection;
                //SS2Log.Info("Haven't started");
                
            }
            /*if (fixedAge >= duration)
            {
                //if (wasLiedTo)
                //by request of ts <3
                GroundSlamPos(characterBody.footPosition);
                outer.SetNextStateToMain();
            }*/
            else
            {
                if (exeController)
                {
                    if (!exeController.hasOOB)
                    {
                        HandleMovement();
                    }
                    else
                    {
                        outer.SetNextStateToMain(); //emergency "everything has failed" stop slamming
                    }
                }
                // HandleMovement();
            }
  
        }

        public void HandleMovement()
        {
            characterMotor.rootMotion += dashVector * moveSpeedStat * 18.5f * Time.fixedDeltaTime;
        }

        private void GroundSlamPos(Vector3 position)
        {
            //get number of enemies hit to divide damage
            //LogCore.LogI($"Velocity {hitGroundInfo.velocity}");

            SphereSearch search = new SphereSearch();
            List<HurtBox> hits = new List<HurtBox>();
            List<HealthComponent> hitTargets = new List<HealthComponent>();

            float damage = baseDamageCoefficient;
            float procMultiplier = 1;

            search.ClearCandidates();
            search.origin = position;
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
            {
                damage *= 2f;
                procMultiplier++;
            }

            bool crit = RollCrit();
            BlastAttack blast = new BlastAttack()
            {
                radius = slamRadius,
                procCoefficient = procCoefficient * procMultiplier,
                position = position,
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
            {
                if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
                {
                    EffectManager.SimpleEffect(slamEffectMastery, position, Quaternion.identity, true);
                }
                else
                {
                    EffectManager.SimpleEffect(slamEffect, position, Quaternion.identity, true);
                }
            }

            outer.SetNextStateToMain();
        }

        private void GroundSlam(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            //get number of enemies hit to divide damage
            //LogCore.LogI($"Velocity {hitGroundInfo.velocity}");

            SphereSearch search = new SphereSearch();
            List<HurtBox> hits = new List<HurtBox>();
            List<HealthComponent> hitTargets = new List<HealthComponent>();

            float damage = baseDamageCoefficient;
            float procMultiplier = 1;

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
            {
                damage *= 2f;
                procMultiplier++;
            }

            bool crit = RollCrit();
            BlastAttack blast = new BlastAttack()
            {
                radius = slamRadius,
                procCoefficient = procCoefficient * procMultiplier,
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
            {
                if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
                {
                    EffectManager.SimpleEffect(slamEffectMastery, hitGroundInfo.position, Quaternion.identity, true);
                }
                else
                {
                    EffectManager.SimpleEffect(slamEffect, hitGroundInfo.position, Quaternion.identity, true);
                }
            }
            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            characterBody.hideCrosshair = false;
            if (exeController != null)
            {
                exeController.meshExeAxe.SetActive(false);
                exeController.isExec = false;
                exeController.hasOOB = false;
            }
            PlayAnimation("FullBody, Override", "SpecialImpact", "Special.playbackRate", duration);
            characterMotor.onHitGroundAuthority -= GroundSlam;
            characterBody.bodyFlags -= CharacterBody.BodyFlags.IgnoreFallDamage;
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 1.2f);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}

