using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Skills;
namespace EntityStates.Pyro
{
    public class PyroLiftoff : BaseState
    {
        private static float blastRadius = 10f;
        private static float damageCoefficient = 3.5f;
        private static float procCoefficient = 1f;
        private static float force = 400f;

        private static float minChargeUpwardVelocity = 20f;
        private static float maxChargeUpwardVelocity = 44f;
        private static float minChargeForwardVelocity = 3f;
        private static float maxChargeForwardVelocity = 1f;
        private static float duration = 0.3f;

        private static string enterSoundString = "";
        public static GameObject launchEffectPrefab;

        private static float airControl = 0.15f;

        public float charge;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            characterMotor.Motor.ForceUnground();
            characterMotor.velocity = Vector3.zero;

            if (isAuthority)
            {
                characterBody.isSprinting = true;

                // do jump
                Vector3 moveVector = inputBank.moveVector;
                Vector3 upwardVelocityVector = Vector3.up * Mathf.Lerp(minChargeUpwardVelocity, maxChargeUpwardVelocity, charge);
                Vector3 forwardVelocityVector = new Vector3(moveVector.x, 0f, moveVector.z).normalized * characterBody.moveSpeed * Mathf.Lerp(minChargeForwardVelocity, maxChargeForwardVelocity, charge);
                characterMotor.Motor.ForceUnground(0.2f);
                characterMotor.velocity = (upwardVelocityVector + forwardVelocityVector);
                characterMotor.airControl = airControl;

                // do blastattack
                DamageTypeCombo dtc = new DamageTypeCombo();
                dtc.damageTypeExtended = DamageTypeExtended.FireNoIgnite;
                dtc.damageSource = DamageSource.Utility;

                BlastAttack launchBlast = new BlastAttack()
                {
                    radius = blastRadius,
                    procCoefficient = procCoefficient,
                    baseDamage = characterBody.damage * damageCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    falloffModel = BlastAttack.FalloffModel.None,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    teamIndex = teamComponent.teamIndex,
                    attacker = gameObject,
                    baseForce = force,
                    position = characterBody.footPosition,
                    damageType = dtc,
                    crit = RollCrit()
                };
                launchBlast.Fire();

                // spawn effect
                if (launchEffectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = characterBody.footPosition,
                        scale = blastRadius,
                        rotation = Util.QuaternionSafeLookRotation(characterMotor.velocity),
                    };
                    EffectManager.SpawnEffect(launchEffectPrefab, effectData, true);
                }

                // enable jetpack
                var bodyStateMachine = FindSiblingStateMachine("Body");
                if (bodyStateMachine)
                {
                    bodyStateMachine.SetInterruptState(new PyroJetpackMain(), InterruptPriority.Pain);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
             
            

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }

    public class PyroJetpackMain : GenericCharacterMain
    {
        public static SkillDef overrideSkillDef;
        private static float airControlRestoration = 0.1f;

        private float originalAirControl;
        private ParticleSystem hoverL;
        private ParticleSystem hoverR;
        public override void OnEnter()
        {
            base.OnEnter();

            hoverL = FindModelChild("HoverLParticles")?.GetComponent<ParticleSystem>();
            hoverR = FindModelChild("HoverRParticles")?.GetComponent<ParticleSystem>();

            originalAirControl = BodyCatalog.GetBodyPrefab(characterBody.bodyIndex).GetComponent<CharacterMotor>().airControl;
            skillLocator.utility.SetSkillOverride(this, overrideSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }
        public override void OnExit()
        {
            base.OnExit();

            if(hoverL)
            {
                hoverL.Stop();
            }
            if (hoverR)
            {
                hoverR.Stop();
            }

            skillLocator.utility.UnsetSkillOverride(this, overrideSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                characterMotor.airControl += Time.fixedDeltaTime * airControlRestoration;
                characterMotor.airControl = Mathf.Clamp(characterMotor.airControl, 0, originalAirControl);
            }

            if (isAuthority && isGrounded && fixedAge >= 0.1f)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
