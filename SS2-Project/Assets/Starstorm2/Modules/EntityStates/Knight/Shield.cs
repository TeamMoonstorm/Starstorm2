using UnityEngine;
using RoR2;
using RoR2.Skills;
using SS2;
using System;
using UnityEngine.Networking;
using MSU.Config;
using EntityStates.BrotherMonster;
using SS2.Components;

namespace EntityStates.Knight
{
    // Knight's Default Secondary
    public class Shield : BaseSkillState
    {
        public static BuffDef shieldBuff;
        public static BuffDef parryBuff;
        public static GameObject parryFlashEffectPrefab;
        public static float parryBuffDuration;
        public static SkillDef shieldBashSkillDef;
        private bool hasParried = false;
        private float stopwatch = 0f;
        public static float minDuration = 0.2f;

        public bool overridden;

        private Animator animator;

        private bool useAltCamera = false;
        private bool isParrying = false;
        private CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private EntityStateMachine rollStateMachine;
        private GameObject parryHurtbox;
        private KnightBlockTracker blockTracker;

        public override void OnEnter()
        {
            base.OnEnter();
            rollStateMachine = EntityStateMachine.FindByCustomName(this.characterBody.gameObject, "Roll");
            parryHurtbox = FindModelChildGameObject("ParryHurtbox");
            blockTracker = characterBody.GetComponent<KnightBlockTracker>();
            animator = GetModelAnimator();

            PlayCrossfade("Gesture, Override", "RaiseShield", 0.1f);
            //animator.SetBool("shieldUp", true);

            characterBody.SetAimTimer(0.5f);

            if (NetworkServer.active)
            {
                characterBody.AddBuff(shieldBuff);
            }

            GameObject attacker = blockTracker.GetLastAttacker();
            if (attacker != null)
            {
                //an attack has been detected slightly before activating
                ParryAttacker(attacker);
            }
            else
            {
                BeginParrying();
            }

            CameraSwap();
        }

        private void CameraSwap()
        {
            if (useAltCamera)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.2f);

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = SS2.Survivors.Knight.altCameraParams,
                    priority = 0f
                };

                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.2f);
            }
            else
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.2f);

                CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = SS2.Survivors.Knight.chargeCameraParams,
                    priority = 0f
                };

                camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.2f);
            }
        }

        private void SetPrimaryOverride(bool shouldSet)
        {
            if (overridden == shouldSet)
                return;
            overridden = shouldSet;

            if (shouldSet)
            {
                skillLocator.primary.SetSkillOverride(skillLocator.primary, shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
            else
            {
                skillLocator.primary.UnsetSkillOverride(skillLocator.primary, shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private void BeginParrying()
        {
            if (isParrying)
                return;
            isParrying = true;

            blockTracker.onIncomingDamageAuthority += ParryAttacker;
            parryHurtbox.SetActive(true);
            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            // TODO
            //EffectData effectData = new EffectData();
            //effectData.origin = this.characterBody.corePosition;
            //EffectManager.SpawnEffect(parryFlashEffectPrefab, effectData, transmit: true);

            //parrytemporaryoverlay
        }

        private void EndParrying()
        {
            if (!isParrying)
                return;
            isParrying = false;

            blockTracker.onIncomingDamageAuthority -= ParryAttacker;
            parryHurtbox.SetActive(false);
            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        private void ParryAttacker(GameObject attacker)
        {
            SetStateOnHurt setStateOnHurt = attacker?.GetComponent<SetStateOnHurt>();
            if (setStateOnHurt)
            {
                // Stun the enemy
                Type state = setStateOnHurt.targetStateMachine.state.GetType();
                if (state != typeof(StunState) && state != typeof(ShockState) && state != typeof(FrozenState))
                {
                    setStateOnHurt.SetStun(3f);
                }
            }

            // TODO: Should we have a custom sound for this?
            Util.PlaySound("NemmandoDecisiveStrikeReady", gameObject);

            EndParrying();
            hasParried = true;
            SetPrimaryOverride(false);
            EntityStateMachine bodyEsm = EntityStateMachine.FindByCustomName(gameObject, "Body");
            if (bodyEsm != null)
            {
                bodyEsm.SetNextState(new EntityStates.Knight.Parry());
            }
            outer.SetNextStateToMain();
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.V) && isAuthority)
            {
                useAltCamera = !useAltCamera;
                CameraSwap();
            }

#if DEBUG
            if (Input.GetKeyDown(KeyCode.F))
            {
                ParryAttacker(null);
            }
#endif
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            SetPrimaryOverride(inputBank.skill2.down);



            if (!inputBank.skill2.down && fixedAge > minDuration)
            {
                outer.SetNextStateToMain();
                return;
            }


            if (fixedAge >= parryBuffDuration)
            {
                EndParrying();
            }

            stopwatch += fixedAge;
            if (stopwatch >= 0.25f)
            {
                stopwatch = 0f;
                characterBody.SetAimTimer(0.5f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.7f);
            }

            characterBody.SetAimTimer(0.5f);
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);

            characterBody.RemoveBuff(shieldBuff);
            SetPrimaryOverride(false);
            EndParrying();

            rollStateMachine.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
