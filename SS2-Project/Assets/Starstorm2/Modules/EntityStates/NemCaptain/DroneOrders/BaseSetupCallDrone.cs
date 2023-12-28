using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class BaseSetupCallDrone : BaseSkillState
    {
        private NemCaptainController ncc;
        private float dur = 0.1f;
        private float discardDur = 0.6f;
        private bool hasSetPrimary = false;
        [SerializeField]
        public SkillDef primaryOverride;

        //not proud of this solution but hey it works!!
        [SerializeField]
        public bool isRegen;
        [SerializeField]
        public bool isDiscard4Mana;
        [SerializeField]
        public bool isDampen;

        public override void OnEnter()
        {
            base.OnEnter();
            hasSetPrimary = false;
            ncc = characterBody.GetComponent<NemCaptainController>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= dur * 0.5f && IsKeyDownAuthority() == false && !hasSetPrimary && isAuthority)
            {
                hasSetPrimary = true;
                UnsetSkills();
                outer.SetNextState(new ForcedCooldown());
                Debug.Log("setting primary");
            }

            if (fixedAge > discardDur && isAuthority)
            {
                UnsetSkills();
                outer.SetNextState(new ForcedCooldown());
                Util.PlaySound("ExecutionerGainCharge", gameObject);
                Debug.Log("discarding");
            }
        }

        public void UnsetSkills()
        {
            if (isAuthority)
            {
                EntityStateMachine esm = EntityStateMachine.FindByCustomName(gameObject, "Skillswap");
                if (esm)
                    esm.SetNextStateToMain();

                //skillLocator.primary.SetSkillOverride(gameObject, activatorSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);

                activatorSkillSlot.UnsetSkillOverride(gameObject, activatorSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                activatorSkillSlot.SetSkillOverride(gameObject, ncc.nullSkill, GenericSkill.SkillOverridePriority.Loadout);

                if (hasSetPrimary)
                {

                    if (isRegen)
                        characterBody.AddTimedBuffAuthority(SS2Content.Buffs.bdNemCapManaRegen.buffIndex, 15f);

                    if (isDampen)
                        characterBody.AddTimedBuffAuthority(SS2Content.Buffs.bdNemCapManaReduction.buffIndex, 10f);

                    if (isDiscard4Mana)
                    {
                        if (ncc.hand1.skillDef != ncc.nullSkill)
                        {
                            ncc.hand1.UnsetSkillOverride(gameObject, ncc.hand1.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                            ncc.hand1.SetSkillOverride(gameObject, ncc.nullSkill, GenericSkill.SkillOverridePriority.Loadout);

                            if (activatorSkillSlot != ncc.hand1)
                                ncc.AddStress(-17f);
                        }

                        if (ncc.hand2.skillDef != ncc.nullSkill)
                        {
                            ncc.hand2.UnsetSkillOverride(gameObject, ncc.hand2.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                            ncc.hand2.SetSkillOverride(gameObject, ncc.nullSkill, GenericSkill.SkillOverridePriority.Loadout);

                            if (activatorSkillSlot != ncc.hand2)
                                ncc.AddStress(-17f);
                        }

                        if (ncc.hand3.skillDef != ncc.nullSkill)
                        {
                            ncc.hand3.UnsetSkillOverride(gameObject, ncc.hand3.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                            ncc.hand3.SetSkillOverride(gameObject, ncc.nullSkill, GenericSkill.SkillOverridePriority.Loadout);

                            if (activatorSkillSlot != skillLocator.utility)
                                ncc.AddStress(-17f);
                        }

                        if (ncc.hand4.skillDef != ncc.nullSkill)
                        {
                            ncc.hand4.UnsetSkillOverride(gameObject, ncc.hand4.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                            ncc.hand4.SetSkillOverride(gameObject, ncc.nullSkill, GenericSkill.SkillOverridePriority.Loadout);

                            if (activatorSkillSlot != ncc.hand4)
                                ncc.AddStress(-17f);
                        }
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (hasSetPrimary && primaryOverride != null)
                skillLocator.primary.SetSkillOverride(gameObject, primaryOverride, GenericSkill.SkillOverridePriority.Replacement);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
