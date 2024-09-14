﻿using SS2;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using MSU;

namespace EntityStates.NemCommando
{
    //This might be stupid, and there might be a better way of using the token modifier here, but the damageCoefficient is a base field and i dont know if its possible to add attributes to inherited fields, so this exists now.
    //★ idk what's going on but i don't think i broke nor fixed whatever you did
    class SwingSword2 : BasicMeleeAttack, SteppedSkillDef.IStepSetter, ISkillState
    {
        public static float swingTimeCoefficient = 1.71f;
        private static float reloadDelay = 0.5f;
        [FormatToken("SS2_NEMMANDO_PRIMARY_BLADE_DESCRIPTION", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SwingSword2().damageCoefficient;
        public int swingSide;
        private bool inCombo = false;
        private string skinNameToken;

        private EntityStateMachine gunSM;
        private NetworkStateMachine nsm;
        public GenericSkill activatorSkillSlot { get; set; }

        public override void OnEnter()
        {
            skinNameToken = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken;

            nsm = GetComponent<NetworkStateMachine>();
            gunSM = nsm.stateMachines[2];

            Idle nextState = new Idle();
            gunSM.SetInterruptState(nextState, InterruptPriority.Skill);

            /*if(skillLocator.secondary && skillLocator.secondary.skillDef is SS2ReloadSkillDef) // theres better syntax for casting but i stupet :(
            {
                (skillLocator.secondary.skillDef as SS2ReloadSkillDef).SetDelayTimer(skillLocator.secondary, reloadDelay);
            }*/

            //red default
            swingEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoSwingEffect", SS2Bundle.NemCommando);
            hitEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoImpactSlashEffect", SS2Bundle.NemCommando);

            //Yellow
            if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_MASTERY")
            {
                swingEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoSwingEffectYellow", SS2Bundle.NemCommando);
                hitEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoImpactSlashEffectYellow", SS2Bundle.NemCommando);
            }
            //Blue
            if (skinNameToken == "SS2_SKIN_NEMCOMMANDO_COMMANDO")
            {
                swingEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoSwingEffectBlue", SS2Bundle.NemCommando);
                hitEffectPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoImpactSlashEffectBlue", SS2Bundle.NemCommando);
            }

            base.OnEnter();

            animator = GetModelAnimator();
        }

        public override void PlayAnimation()
        {
            //string animationStateName = (swingSide == 0) ? "Primary1TreeNew" : "Primary2TreeNew";
            //PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, duration * swingTimeCoefficient * 0.2f);   
            /*string animString = "Primary" + (1 + swingSide).ToString();
            if (inCombo)
            {
                PlayCrossfade("Gesture, Override", "Primary3", "Primary.playbackRate", duration * swingTimeCoefficient, duration * swingTimeCoefficient * 0.1f);
                Debug.Log(swingSide + " - swingSide");
            }
            else
            {
                PlayCrossfade("Gesture, Override", animString, "Primary.playbackRate", duration * swingTimeCoefficient, duration * swingTimeCoefficient * 0.1f);
                Debug.Log(swingSide + " - swingSide");
            }*/

            string animString = "Primary" + (1 + swingSide).ToString();
            if (inCombo)
                animString = "Primary3";

            PlayCrossfade("Gesture, Override", animString, "Primary.playbackRate", duration * swingTimeCoefficient, 0.05f);
            //Debug.Log("swingSide: " + swingSide);
        }

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            swingSide = i;
            if (swingSide > 1)
            {
                ((SteppedSkillDef.InstanceData)activatorSkillSlot.skillInstanceData).step = 0;
                swingSide = 0;
                inCombo = true;
            }

            swingEffectMuzzleString = (swingSide == 0) ? "SwingLeft" : "SwingRight";
        }

        public override void OnExit()
        {
            base.OnExit();
            Idle nextState = new Idle();
            gunSM.SetInterruptState(nextState, InterruptPriority.Skill);
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)swingSide);
            this.Serialize(skillLocator, writer);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swingSide = (int)reader.ReadByte();
            this.Deserialize(skillLocator, reader);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            DamageAPI.AddModdedDamageType(overlapAttack, SS2.Survivors.NemCommando.GougeDamageType);
        }
    }
}