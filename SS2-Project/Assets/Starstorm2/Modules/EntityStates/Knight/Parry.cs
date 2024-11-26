using MSU;
using MSU.Config;
using RoR2;
using RoR2.Skills;
using SS2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Knight
{
    // Parry state that is entered/triggered from the bdKnightParryBuff code.
    public class Parry : BasicMeleeAttack
    {
        [FormatToken("SS2_KNIGHT_SHIELD_BASH_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        
        // TODO: Make static once you have a replacement
        public GameObject impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBomb.prefab").WaitForCompletion();

        //todo wish I had time to put these on the skilldef like GI
        public static SkillDef buffedPrimarySkillDef;
        public static SkillDef buffedUtilitySkillDef;
        public static SkillDef buffedSpecialSkillDef;

        public int swingSide;

        private GenericSkill originalPrimarySkill;
        private GenericSkill originalUtilitySkill;
        private GenericSkill originalSpecialSkill;


        // Movement variables
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testbaseDuration = 0.69f;

        public override void OnEnter()
        {
            baseDuration = testbaseDuration;

            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            animator = GetModelAnimator();

            // Grab and set the original skills
            originalPrimarySkill = skillLocator.primary;
            originalUtilitySkill = skillLocator.utility;
            originalSpecialSkill = skillLocator.special;

            // Assign the buffed skill versions
            originalPrimarySkill.SetSkillOverride(gameObject, buffedPrimarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
            originalUtilitySkill.SetSkillOverride(gameObject, buffedUtilitySkillDef, GenericSkill.SkillOverridePriority.Contextual);
            originalSpecialSkill.SetSkillOverride(gameObject, buffedSpecialSkillDef, GenericSkill.SkillOverridePriority.Contextual);

            EffectData effectData = new EffectData();
            effectData.origin = this.characterBody.corePosition;
            EffectManager.SpawnEffect(impactEffect, effectData, transmit: false);
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("FullBody, Override", "Parry", "Secondary.playbackRate", duration, 0.15f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            inputBank.moveVector = Vector3.zero;
            characterMotor.velocity = Vector3.zero;

            PerformInputs();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        protected void PerformInputs()
        {
            if (base.isAuthority)
            {
                    HandleSkill(base.skillLocator.primary, ref base.inputBank.skill1);
                    //HandleSkill(base.skillLocator.secondary, ref base.inputBank.skill2);
                    HandleSkill(base.skillLocator.utility, ref base.inputBank.skill3);
                    HandleSkill(base.skillLocator.special, ref base.inputBank.skill4);
            }
            void HandleSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
            {
                if (buttonState.down && (bool)skillSlot && (!skillSlot.mustKeyPress || !buttonState.hasPressBeenClaimed) && skillSlot.ExecuteIfReady())
                {
                    buttonState.hasPressBeenClaimed = true;
                    base.outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            //if (inputBank.skill2.down)
            //{
            //    outer.SetNextState(new Shield());
            //} 
            //else
            //{
            //    outer.SetNextStateToMain();
            //}

            // Assign the buffed skill versions
            originalPrimarySkill.UnsetSkillOverride(gameObject, buffedPrimarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
            originalUtilitySkill.UnsetSkillOverride(gameObject, buffedUtilitySkillDef, GenericSkill.SkillOverridePriority.Contextual);
            originalSpecialSkill.UnsetSkillOverride(gameObject, buffedSpecialSkillDef, GenericSkill.SkillOverridePriority.Contextual);


            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            
            base.OnExit();
        }
        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}