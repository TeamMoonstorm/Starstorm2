using Moonstorm;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Knight
{
    // Parry state that is entered/triggered from the bdKnightParryBuff code.
    public class Parry : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_KNIGHT_SHIELD_BASH_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        
        // TODO: Make static once you have a replacement
        public GameObject impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBomb.prefab").WaitForCompletion();

        public static SkillDef buffedPrimarySkill;
        public static SkillDef buffedUtilitySkill;
        public static SkillDef buffedSpecialSkill;

        public int swingSide;

        private GenericSkill originalPrimarySkill;
        private GenericSkill originalUtilitySkill;
        private GenericSkill originalSpecialSkill;


        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            animator = GetModelAnimator();

            // Grab and set the original skills
            originalPrimarySkill = skillLocator.primary;
            originalUtilitySkill = skillLocator.utility;
            originalSpecialSkill = skillLocator.special;

            //var buffedPrimary = originalPrimarySkill.GetFieldValue<SkillDef>("buffedSkillRef");

            //Debug.Log("buffedPrimary : " + buffedPrimary.name);

            // Assign the buffed skill versions
            originalPrimarySkill.SetSkillOverride(gameObject, buffedPrimarySkill, GenericSkill.SkillOverridePriority.Contextual);
            originalUtilitySkill.SetSkillOverride(gameObject, buffedUtilitySkill, GenericSkill.SkillOverridePriority.Contextual);
            originalSpecialSkill.SetSkillOverride(gameObject, buffedSpecialSkill, GenericSkill.SkillOverridePriority.Contextual);
            Debug.Log("setting buffed skills");

            EffectData effectData = new EffectData();
            effectData.origin = this.characterBody.corePosition;
            EffectManager.SpawnEffect(impactEffect, effectData, transmit: false);
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "Parry", "Secondary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            if (inputBank.skill2.down)
            {
                outer.SetNextState(new Shield());
            }

            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

            outer.SetNextStateToMain();
            base.OnExit();
        }
        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}