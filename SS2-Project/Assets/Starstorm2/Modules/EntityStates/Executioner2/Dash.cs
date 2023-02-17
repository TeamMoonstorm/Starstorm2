using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EntityStates.Executioner2
{
    public class Dash : BaseSkillState
    {
        public static float baseDuration = 0.6f;
        public static float speedMultiplier = 3.0f;
        public static float debuffRadius = 14f;
        [TokenModifier("SS2_EXECUTIONER_DASH_DESCRIPTION", StatTypes.Default, 0)]
        public static float debuffDuration = 4.0f;
        public static float debuffCheckInterval = 0.0333333f;
        public static float hopVelocity; // = 17f;

        private float debuffCheckStopwatch;
        private float duration;
        private SphereSearch fearSearch;
        private List<HurtBox> hits;
        private Animator animator;

        public static GameObject dashEffect;
        public static string ExhaustL;
        public static string ExhaustR;


        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();

            debuffCheckStopwatch = 0f;
            duration = baseDuration;
            Util.PlayAttackSpeedSound("ExecutionerUtility", gameObject, 1.0f);
            PlayAnimation("FullBody, Override", "Utility", "Utility.playbackRate", duration * 1.65f);

            HopIfAirborne();

            hits = new List<HurtBox>();
            fearSearch = new SphereSearch();
            fearSearch.mask = LayerIndex.entityPrecise.mask;
            fearSearch.radius = debuffRadius;

            //create dash aoe
            if (isAuthority)
            {
                CreateFearAoe();
            }

            EffectManager.SimpleMuzzleFlash(dashEffect, gameObject, ExhaustL, false);
            EffectManager.SimpleMuzzleFlash(dashEffect, gameObject, ExhaustR, false);

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 1.5f * baseDuration;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matHuntressFlashBright");
                temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
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
                if (hp)
                {
                    SetStateOnHurt ssoh = hp.GetComponent<SetStateOnHurt>();
                    if (ssoh)
                    {
                        Type state = ssoh.targetStateMachine.state.GetType();
                        if (state != typeof(EntityStates.StunState) && state != typeof(EntityStates.ShockState) && state != typeof(EntityStates.FrozenState))
                        {
                            ssoh.SetStun(-1f);
                        }
                    }

                    CharacterBody body = hp.body;
                    if (body && body != base.characterBody)
                    {
                        body.AddTimedBuff(SS2Content.Buffs.BuffFear, debuffDuration);
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.isSprinting = true;

            if (characterDirection && characterMotor)
                characterMotor.rootMotion += characterDirection.forward * characterBody.moveSpeed * speedMultiplier * Time.fixedDeltaTime;

            if (isAuthority)
            {
                debuffCheckStopwatch += Time.fixedDeltaTime;
                if (debuffCheckStopwatch >= debuffCheckInterval)
                {
                    debuffCheckInterval -= debuffCheckInterval;
                    CreateFearAoe();
                }
            }

            if (fixedAge >= duration)
                outer.SetNextStateToMain();
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
