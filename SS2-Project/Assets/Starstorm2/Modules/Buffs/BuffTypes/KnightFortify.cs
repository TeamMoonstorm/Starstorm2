using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class KnightFortify : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdFortified", SS2Bundle.Indev);

        public override Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matKnightShield", SS2Bundle.Indev);
        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier, IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdFortified;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += 100f;
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (damageReport.attacker != damageReport.victim && damageReport.attacker != null && !damageReport.isFallDamage && !damageReport.damageInfo.rejected && !body.HasBuff(SS2Content.Buffs.bdParry.buffIndex))
                {
                    body.SetBuffCount(SS2Content.Buffs.bdFortified.buffIndex, buffStacks -= 1);
                }
            }

            public void FixedUpdate()
            {
                if (buffStacks > 3)
                    buffStacks = 3;
            }
        }
    }
}
