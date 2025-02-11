using SS2;
using SS2.Components;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class BaseSetupCallDrone : BaseSkillState
    {
        private NemCaptainController ncc;
        private float dur = 0.1f;
        private float discardDur = 0.6f;
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
            //ncc = characterBody.GetComponent<NemCaptainController>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= dur * 0.5f && isAuthority)
            {
                UnsetSkills();
                outer.SetNextState(new ForcedCooldown());
                Debug.Log("setting primary");
            }
        }

        public void UnsetSkills()
        {
            if (isAuthority)
            {
                if (isRegen)
                    characterBody.AddTimedBuffAuthority(SS2Content.Buffs.bdNemCapManaRegen.buffIndex, 15f);

                if (isDampen)
                    characterBody.AddTimedBuffAuthority(SS2Content.Buffs.bdNemCapManaReduction.buffIndex, 10f);
                
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
