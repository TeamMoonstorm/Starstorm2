using HG;
using Moonstorm.Components;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
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
                var victim = damageReport.victim;
                var attacker = damageReport.attacker;

                if (victim.gameObject != attacker && damageReport.damageInfo.procCoefficient > 0)
                {
                    victim.body.AddTimedBuffAuthority(SS2Content.Buffs.bdPurplePoison.buffIndex, 6f);

                    /*var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attacker,
                        victimObject = victim.gameObject,
                        dotIndex = PurplePoison.index,
                        duration = damageReport.damageInfo.procCoefficient * 3f,
                        damageMultiplier = 1f
                    };
                    DotController.InflictDot(ref dotInfo);*/
                }
            }
        }
    }
}
