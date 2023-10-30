using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class LampBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdLampBuff", SS2Bundle.Monsters);

        public override Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matLampBuffOverlay", SS2Bundle.Monsters);
        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdLampBuff;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.bdLampBuff) && body.bodyIndex != BodyCatalog.FindBodyIndex("LampBossBody"))
                {
                    args.primaryCooldownMultAdd += 0.5f;
                    args.secondaryCooldownMultAdd += 0.25f;
                    args.damageMultAdd += 0.2f;
                    args.moveSpeedMultAdd += 1f;
                }
            }

            public void OnDestroy()
            {
                body.RecalculateStats();
            }
        }
    }
}
