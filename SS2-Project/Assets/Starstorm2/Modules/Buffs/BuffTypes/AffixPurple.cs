using Moonstorm.Components;
using RoR2;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class AffixPurple : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdElitePurple", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior, IOnDamageDealtServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdElitePurple;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (NetworkServer.active)
                {
                    var victim = damageReport.victim;
                    var attacker = damageReport.attacker;

                    if (victim.gameObject != attacker && damageReport.damageInfo.procCoefficient > 0)
                    {
                        victim.body.AddTimedBuffAuthority(SS2Content.Buffs.bdPoisonBuildup.buffIndex, 8f);
                    }

                    if (victim.gameObject != attacker && victim.body.GetBuffCount(SS2Content.Buffs.bdPoisonBuildup) >= 3)
                    {
                        victim.body.AddTimedBuffAuthority(SS2Content.Buffs.bdPurplePoison.buffIndex, 10f);
                    }
                }
            }
        }
    }
}
