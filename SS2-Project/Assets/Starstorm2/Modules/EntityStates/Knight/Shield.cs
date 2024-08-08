using UnityEngine;
using RoR2;
using RoR2.Skills;
using SS2;

namespace EntityStates.Knight
{
    // Knight's Default Secondary
    public class Shield : BaseState
    {
        public static BuffDef shieldBuff;
        public static BuffDef parryBuff;
        public static GameObject parryFlashEffectPrefab;
        public static float parryBuffDuration;
        public static float minDur;
        public static SkillDef shieldBashSkillDef;
        private bool hasParried = false;
        private float stopwatch = 0f;

        private Animator animator;

        private void SetShieldOverride()
        {
            if (!characterBody.HasBuff(SS2Content.Buffs.bdKnightShieldCooldown))
            {
                skillLocator.primary.SetSkillOverride(skillLocator.primary, shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();

            PlayCrossfade("Gesture, Override", "RaiseShield", 0.1f);
            animator.SetBool("shieldUp", true);

            characterBody.SetAimTimer(0.5f);

            //characterBody.AddTimedBuff(parryBuff, 0.1f);
            characterBody.AddBuff(shieldBuff);

            // This sets the shield bash skill
            SetShieldOverride();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= 0.075f && !hasParried)
            {
                // Remove the shield bash since the parry state will override skills too 
                // skillLocator.primary.UnsetSkillOverride(skillLocator.primary, shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);
                hasParried = true;
                characterBody.AddTimedBuff(parryBuff, parryBuffDuration);

                // TODO
                //EffectData effectData = new EffectData();
                //effectData.origin = this.characterBody.corePosition;
                //EffectManager.SpawnEffect(parryFlashEffectPrefab, effectData, transmit: true);
            }

            stopwatch += fixedAge;
            if (stopwatch >= 0.25f)
            {
                stopwatch = 0f;
                characterBody.SetAimTimer(0.5f);
            }

            if (fixedAge >= minDur && !inputBank.skill2.down)
                outer.SetNextStateToMain(); 
        }

        public override void OnExit()
        {

            if (inputBank.skill2.down)
            {
                outer.SetNextState(new Shield());
                characterBody.RemoveBuff(shieldBuff);
            } 
            else
            {
                animator.SetBool("shieldUp", false);

                characterBody.RemoveBuff(shieldBuff);

                characterBody.SetAimTimer(0.5f);
            }

            // If the player did not parry we need to unset the skill override
            if (!hasParried)
            {
                //skillLocator.primary.UnsetSkillOverride(skillLocator.primary, shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);
            }

            skillLocator.primary.UnsetSkillOverride(skillLocator.primary, shieldBashSkillDef, GenericSkill.SkillOverridePriority.Contextual);

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
