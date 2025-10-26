using SS2;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using MSU;

namespace EntityStates.Executioner2
{
    public class Dash : BaseSkillState
    {
        public static float baseDuration = 0.6f;
        public static float speedMultiplier = 3.0f;
        public static float debuffRadius = 12f;
        [FormatToken("SS2_EXECUTIONER_DASH_DESCRIPTION",   0)]
        public static float debuffDuration = 4.0f;
        public static float debuffCheckInterval = 0.0333333f;
        public static float hopVelocity; // = 17f;
        private static float turnSpeed = 360f; // = 17f;

        private float debuffCheckStopwatch;
        private float duration;
        private SphereSearch fearSearch;
        private List<HurtBox> hits;
        private List<HealthComponent> fearedTargets = new List<HealthComponent>();
        private Animator animator;

        public static GameObject dashEffect;
        public static GameObject dashEffectMastery;
        public static Material dashMasteryMaterial;
        public static string ExhaustL;
        public static string ExhaustR;

        private EntityStateMachine bodyStateMachine;
        private Vector3 currentDirection;
        private Vector3 directionVelocity;

        private bool playedExitAnimation;
        private static float asodiyhbeajbde = 0.33f;
        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            bodyStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
            debuffCheckStopwatch = 0f;
            duration = baseDuration;
            Util.PlayAttackSpeedSound("ExecutionerUtility", gameObject, 1.0f);
            PlayAnimation("FullBody, Override", "Dash", "Utility.playbackRate", duration * asodiyhbeajbde);

            HopIfAirborne();

            characterDirection.turnSpeed = turnSpeed;
            currentDirection = characterDirection.forward;

            hits = new List<HurtBox>();
            fearSearch = new SphereSearch();
            fearSearch.mask = LayerIndex.entityPrecise.mask;
            fearSearch.radius = debuffRadius;

            string skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;
            bool inMasterySkin = skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY";
            //create dash aoe
            if (NetworkServer.active)
            {
                CreateFearAoe();
                
                if (inMasterySkin)
                {
                    EffectManager.SimpleMuzzleFlash(dashEffectMastery, gameObject, ExhaustL, true);
                    EffectManager.SimpleMuzzleFlash(dashEffectMastery, gameObject, ExhaustR, true);
                }
                else
                {
                    EffectManager.SimpleMuzzleFlash(dashEffect, gameObject, ExhaustL, true);
                    EffectManager.SimpleMuzzleFlash(dashEffect, gameObject, ExhaustR, true);
                }
            }

            

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                // yknow i probably should have done !skinNameToken so its easier to understand but this works and i dont wanna change it so idk - b
                if (inMasterySkin)
                {
                    temporaryOverlay.duration = 1.3f * baseDuration; // doing this because otherwise it just makes him flash white ??? i still have no idea why this doesnt happen with the huntress one bc the settings are the same :((( -b
                }
                else
                {
                    temporaryOverlay.duration = 1.5f * baseDuration;
                }

                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;


                if (inMasterySkin)
                {
                    temporaryOverlay.originalMaterial = dashMasteryMaterial;
                }
                else
                {
                    temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlashBright.mat").WaitForCompletion();
                }

                // TODO: No longer needed post-SOTS, leaving in for now but need to remove later
                //temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }
        }

        private void CreateFearAoe()
        {
            hits.Clear();
            fearSearch.ClearCandidates();
            fearSearch.origin = characterBody.corePosition;
            fearSearch.RefreshCandidates();
            fearSearch.FilterCandidatesByDistinctHurtBoxEntities();
            fearSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(teamComponent.teamIndex));
            fearSearch.GetHurtBoxes(hits);
            foreach (HurtBox h in hits)
            {
                HealthComponent hp = h.healthComponent;
                if (hp && !fearedTargets.Contains(hp))
                {
                    fearedTargets.Add(hp);
                    SetStateOnHurt ssoh = hp.GetComponent<SetStateOnHurt>();
                    if (ssoh)
                    {
                        ssoh.SetStun(-1f);
                    }

                    CharacterBody body = hp.body;
                    if (body && body != base.characterBody)
                    {
                        body.AddTimedBuff(SS2Content.Buffs.BuffFear, debuffDuration);

                        if(body.master && body.master.aiComponents.Length > 0 && body.master.aiComponents[0])
                        {
                            body.master.aiComponents[0].stateMachine.SetNextState(new AI.Walker.Fear { fearTarget = base.gameObject });
                        }
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            characterDirection.turnSpeed = 720f;
            if (!playedExitAnimation)
            {
                PlayAnimation("FullBody, Override", "BufferEmpty");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.isSprinting = true;

            bool inExecuteLeap = bodyStateMachine && bodyStateMachine.state is ExecuteLeap;
            Vector3 direction = inExecuteLeap ? inputBank.moveVector : characterDirection.forward;
            if(inExecuteLeap)
            {
                playedExitAnimation = true; // no exit anim if slammin
                direction = Vector3.SmoothDamp(currentDirection, direction, ref directionVelocity, 360f / turnSpeed * 0.25f);
                currentDirection = direction;
            }
            else
            {
                currentDirection = characterDirection.forward;
            }

            if (characterDirection && characterMotor)
                characterMotor.rootMotion += direction * characterBody.moveSpeed * speedMultiplier * Time.fixedDeltaTime;

            if (NetworkServer.active)
            {
                debuffCheckStopwatch += Time.fixedDeltaTime;
                if (debuffCheckStopwatch >= debuffCheckInterval)
                {
                    debuffCheckInterval -= debuffCheckInterval;
                    CreateFearAoe();
                }
            }

            if (fixedAge >= duration)
            {
                if (!playedExitAnimation)
                {
                    PlayAnimation("FullBody, Override", "DashEnd");
                    playedExitAnimation = true;
                }

                if(isAuthority && !inExecuteLeap)
                {
                    outer.SetNextStateToMain();
                }
                    
            }
                
        }
        private void HopIfAirborne()
        {
            if (!characterMotor.isGrounded)
            {
                SmallHop(characterMotor, hopVelocity);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
