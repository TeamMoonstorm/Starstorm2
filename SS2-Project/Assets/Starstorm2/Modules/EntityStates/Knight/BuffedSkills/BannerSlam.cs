using EntityStates;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;
using MSU.Config;
using SS2;

namespace EntityStates.Knight
{
    public class EnterBannerSlam : EnterBannerSpecial
    {
        public override void SetNextState()
        {
            outer.SetNextState(new BannerSlam());
        }
    }
    public class BannerSlam : BannerSpecial
    {
        private static float barrierMultiplier = 0.4f;

        protected override void FireImpact()
        {
            base.FireImpact();

            for (int i = 0; i < CharacterBody.readOnlyInstancesList.Count; i++)
            {
                CharacterBody character = CharacterBody.readOnlyInstancesList[i];
                if (character.teamComponent.teamIndex == characterBody.teamComponent.teamIndex &&
                    (character.transform.position - transform.position).sqrMagnitude < impactRadius * impactRadius)
                {
                    character.healthComponent.AddBarrier(characterBody.healthComponent.fullCombinedHealth * barrierMultiplier);
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public override void OnExit()
        {
            base.OnExit();

            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }
    }

}