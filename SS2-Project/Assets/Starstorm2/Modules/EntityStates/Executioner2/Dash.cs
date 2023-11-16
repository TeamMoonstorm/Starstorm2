using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

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
        public static GameObject dashEffectMastery;
        public static Material dashMasteryMaterial;
        public static string ExhaustL;
        public static string ExhaustR;

        private string skinNameToken;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();

            debuffCheckStopwatch = 0f;
            duration = baseDuration;
            Util.PlayAttackSpeedSound("ExecutionerUtility", gameObject, 1.0f);
            PlayAnimation("FullBody, Override", "Utility", "Utility.playbackRate", duration * 1.65f);

            HopIfAirborne();

            characterDirection.turnSpeed = 360f;

            hits = new List<HurtBox>();
            fearSearch = new SphereSearch();
            fearSearch.mask = LayerIndex.entityPrecise.mask;
            fearSearch.radius = debuffRadius;

            //create dash aoe
            if (NetworkServer.active)
            {
                CreateFearAoe();
            }

            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
            {
                EffectManager.SimpleMuzzleFlash(dashEffectMastery, gameObject, ExhaustL, true);
                EffectManager.SimpleMuzzleFlash(dashEffectMastery, gameObject, ExhaustR, true);
            }
            else
            {
                EffectManager.SimpleMuzzleFlash(dashEffect, gameObject, ExhaustL, true);
                EffectManager.SimpleMuzzleFlash(dashEffect, gameObject, ExhaustR, true);
            }

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                // yknow i probably should have done !skinNameToken so its easier to understand but this works and i dont wanna change it so idk - b
                if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
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


                if (skinNameToken == "SS2_SKIN_EXECUTIONER2_MASTERY")
                {
                    temporaryOverlay.originalMaterial = dashMasteryMaterial;
                }
                else
                {
                    temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlashBright.mat").WaitForCompletion();
                }

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
                        if (state != typeof(StunState) && state != typeof(ShockState) && state != typeof(FrozenState))
                        {
                            ssoh.SetStun(-1f);
                        }
                    }

                    CharacterBody body = hp.body;
                    if (body && body != base.characterBody)
                    {
                        body.AddTimedBuff(SS2Content.Buffs.BuffFear, debuffDuration);

                        if(body.master && body.master.aiComponents[0])
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
