using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class Insecticide : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Insecticide", SS2Bundle.Items);
        public static DotController.DotIndex DotIndex;
        public static float duration = 3;

        public override void Initialize()
        {
            DotController.onDotInflictedServerGlobal += RefreshInsects;
        }

        private void RefreshInsects(DotController dotController, ref InflictDotInfo inflictDotInfo)
        {
            if (inflictDotInfo.dotIndex == DotIndex)
            {
                int i = 0;
                int count = dotController.dotStackList.Count;

                while (i < count)
                {
                    if (dotController.dotStackList[i].dotIndex == DotIndex)
                    {
                        dotController.dotStackList[i].timer = Mathf.Max(dotController.dotStackList[i].timer, duration);
                    }
                    i++;
                }
            }
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Insecticide;
            public void OnDamageDealtServer(DamageReport report)
            {
                if (report.victimBody.teamComponent.teamIndex != report.attackerBody.teamComponent.teamIndex && report.damageInfo.procCoefficient > 0)
                {
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = body.gameObject,
                        victimObject = report.victim.gameObject,
                        dotIndex = Buffs.Insecticide.index,
                        duration = report.damageInfo.procCoefficient * duration,
                        damageMultiplier = stack
                    };
                    DotController.InflictDot(ref dotInfo);
                }
            }
        }
    }
}
