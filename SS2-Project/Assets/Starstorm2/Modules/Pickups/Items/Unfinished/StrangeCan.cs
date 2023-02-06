using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class StrangeCan : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("StrangeCan", SS2Bundle.Items);

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = false)]
            private static ItemDef GetItemDef() => SS2Content.Items.StrangeCan;

            public void OnDamageDealtServer(DamageReport report)
            {
                Util.PlaySound("StrangeCan", report.victim.gameObject);
                report.victimBody.AddTimedBuff(Buffs.Intoxicated.buff, 5);
                bool flag = (report.damageInfo.damageType & DamageType.PoisonOnHit) > DamageType.Generic;
                if (flag || Util.CheckRoll(8.05f + stack * report.damageInfo.procCoefficient, body.master))
                {
                    /*ProcChainMask procChainMask = report.damageInfo.procChainMask;
                    procChainMask.AddProc(ProcType.BleedOnHit);
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = body.gameObject,
                        victimObject = report.victim.gameObject,
                        dotIndex = Buffs.StrangeCan.index,
                        duration = StaticValues.canDuration,
                        damageMultiplier = StaticValues.canDamage
                    };
                    DotController.InflictDot(ref dotInfo);*/
                    //body.AddTimedBuff(Buffs.Intoxicated.buff, 10f, 5);
                }
            }
        }
    }
}
