/*using EntityStates;
using RoR2;
using UnityEngine;

namespace EntityStates.Nucleator
{
    class ChargeFissionImpulse : NucleatorSkillStateBase
    {
        private Transform modelTransform;
        private HurtBoxGroup hurtboxGroup;
        private CharacterModel characterModel;

        public override void OnEnter()
        {
            base.OnEnter();
            modelTransform = GetModelTransform();
            if (modelTransform)
            {
                characterModel = modelTransform.GetComponent<CharacterModel>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }
            if (hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            PlayAnimation("FullBody, Override", "UtilityCharge", "Utility.playbackRate", 0.9f * maxChargeTime);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((fixedAge >= maxChargeTime || !inputBank || !inputBank.skill3.down) && isAuthority)
            {
                outer.SetNextState(new FireFissionImpulse(hurtboxGroup, characterModel, charge));
                return;
            }
        }



        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}

*/