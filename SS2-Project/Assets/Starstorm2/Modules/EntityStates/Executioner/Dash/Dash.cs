using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Executioner
{
    public class Dash : BaseSkillState
    {
        public static float baseDuration = 0.9f;
        public static float speedMultiplier = 3.0f;
        public static float debuffRadius = 20f;
        [TokenModifier("SS2_EXECUTIONER_DASH_DESCRIPTION", StatTypes.Default, 0)]
        public static float debuffDuration = 4.0f;
        public static GameObject dashEffect;
        public static float debuffCheckInterval = 0.0333333333f;
        public static float hopVelocity; // = 17f;
        private float debuffCheckStopwatch;

        private float duration;
        private SphereSearch fearSearch;
        private List<HurtBox> hits;
        private Animator animator;


        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();

            duration = baseDuration;
            Util.PlayAttackSpeedSound("ExecutionerUtility", gameObject, 1.0f);
            PlayAnimation("FullBody, Override", "Utility", "Utility.playbackRate", duration * 1.75f);
            debuffCheckStopwatch = 0f;
            HopIfAirborne();

            //dash effect
            if (dashEffect)
                EffectManager.SimpleMuzzleFlash(dashEffect, gameObject, "DashEffect", true);

            hits = new List<HurtBox>();
            fearSearch = new SphereSearch();
            fearSearch.mask = LayerIndex.entityPrecise.mask;
            fearSearch.radius = debuffRadius;

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 1.5f * baseDuration;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
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

                //thanks moffein
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

                    CharacterBody body = hp?.body;
                    if (body && body != characterBody && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.baseNameToken != "BROTHER_BODY_NAME" && body.baseNameToken != "ULTRAMITH_NAME")
                    {
                        /*var fear = body.AddItemBehavior<Fear.Behavior>(1);
                        fear.inflictor = characterBody;*/
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

            if (NetworkServer.active)
            {
                debuffCheckStopwatch += Time.fixedDeltaTime;
                if (debuffCheckStopwatch >= debuffCheckInterval)
                {
                    debuffCheckStopwatch -= debuffCheckInterval;
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
