using Moonstorm.Components;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class StormBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffStorm", SS2Bundle.Events);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffStorm;
            public DeathRewards deathRewards;
            public uint ogGoldReward;
            public uint ogExpReward;
            public void Start()
            {
                deathRewards = body.GetComponent<DeathRewards>();
                if (deathRewards)
                {
                    ogGoldReward = deathRewards.goldReward;
                    ogExpReward = deathRewards.expReward;

                    deathRewards.goldReward = (uint)(ogGoldReward * 2f);
                    deathRewards.expReward = (uint)(ogExpReward * 2.5f);
                }
            }
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //args.armorAdd += body.levelArmor * Run.instance.difficultyCoefficient; //what?
                args.armorAdd += 20f;
                //args.critAdd += 20f;
                args.damageMultAdd += 0.2f;
                args.attackSpeedMultAdd += 0.5f;
                args.moveSpeedMultAdd += 0.5f;
                args.cooldownMultAdd += 0.2f;
            }
            public void OnDestroy()
            {
                if (deathRewards)
                {
                    deathRewards.goldReward = ogGoldReward;
                    deathRewards.expReward = ogExpReward;
                }
            }
        }
    }
}
