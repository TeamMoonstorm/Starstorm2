using MSU;
using MSU.Config;
using RoR2;
using RoR2.Skills;
using SS2;
using SS2.Components;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Knight
{
    // Parry state that is entered/triggered from the bdKnightParryBuff code.
    public class Parry : BasicMeleeAttack
    {
        [SerializeField]
        private float inputtableTime;

        [FormatToken("SS2_KNIGHT_SHIELD_BASH_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        
        // TODO: Make static once you have a replacement
        public GameObject impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBomb.prefab").WaitForCompletion();
        public int swingSide;

        private GenericSkill primarySkill;
        private GenericSkill utilitySkill;
        private GenericSkill specialSkill;

        private SkillDef buffedPrimarySkillDef;
        private SkillDef buffedUtilitySkillDef;
        private SkillDef buffedSpecialSkillDef;

        private UpgradedSkillCache upgradedSkillCache;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testbaseDuration = 0.69f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testInputLockDuration = 0.69f;

        public override void OnEnter()
        {
            baseDuration = testbaseDuration;
            inputtableTime = testInputLockDuration;

            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            animator = GetModelAnimator();

            // Grab and set the original skills
            primarySkill = skillLocator.primary;
            utilitySkill = skillLocator.utility;
            specialSkill = skillLocator.special;

            upgradedSkillCache = gameObject.GetComponent<UpgradedSkillCache>();

            TryOverrideSkill(primarySkill, ref buffedPrimarySkillDef);
            TryOverrideSkill(utilitySkill, ref buffedUtilitySkillDef);
            TryOverrideSkill(specialSkill, ref buffedSpecialSkillDef);

            EffectData effectData = new EffectData();
            effectData.origin = this.characterBody.corePosition;
            EffectManager.SpawnEffect(impactEffect, effectData, transmit: false);
        }

        private void TryOverrideSkill(GenericSkill genericSkill, ref SkillDef buffedSkillDef)
        {
            if(upgradedSkillCache == null)
            {
                SS2Log.Warning($"no upgradedskillcachecomponent. doing a probably costly search through the skillcatalog");
                buffedSkillDef = UpgradedSkillCache.FindUpgradedSkillDef(genericSkill.skillDef);
            }
            else
            {
                buffedSkillDef = upgradedSkillCache.GetUpgradedSkillDef(genericSkill.skillDef);
            }

            if (buffedSkillDef == null)
                return;

            SetOverride(genericSkill, buffedSkillDef, true);
        }

        private void SetOverride(GenericSkill genericSkill, SkillDef buffedSkillDef, bool shouldSet)
        {
            if (shouldSet)
            {
                genericSkill.SetSkillOverride(gameObject, buffedSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            } 
            else
            {
                genericSkill.UnsetSkillOverride(gameObject, buffedSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public override void PlayAnimation()
        {
            PlayAnimation("FullBody, Override", "Parry", "Secondary.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            inputBank.moveVector = Vector3.zero;
            characterMotor.velocity = Vector3.zero;


            if (fixedAge >= inputtableTime * duration)
            {
                PerformInputs();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        //ripped from genericcharactermain
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

            TryUnsetOverride(primarySkill, buffedPrimarySkillDef);
            TryUnsetOverride(utilitySkill, buffedUtilitySkillDef);
            TryUnsetOverride(specialSkill, buffedSpecialSkillDef);

            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            
            base.OnExit();
        }

        private void TryUnsetOverride(GenericSkill primarySkill, SkillDef buffedPrimarySkillDef)
        {
            if(buffedPrimarySkillDef == null)
                return;

            SetOverride(primarySkill, buffedPrimarySkillDef, false);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}