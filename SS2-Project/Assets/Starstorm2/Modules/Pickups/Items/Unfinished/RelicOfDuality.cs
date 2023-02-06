using RoR2;
using RoR2.Items;
using System;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class RelicOfDuality : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfDuality", SS2Bundle.Indev);

        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver, IOnDamageDealtServerReceiver
        {
            //we freeze when hit
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfDuality;

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                SetStateOnHurt setStateOnHurt = body.gameObject.GetComponent<SetStateOnHurt>();
                //if (setStateOnHurt.canBeFrozen)
                {
                    //add 1f so there's a minimum freeze time
                    setStateOnHurt.SetFrozen((damageInfo.procCoefficient + 1f) * 6f);
                }
            }
            //don't forget to grab this shit! just stole from green chocolate
            private void Start()
            {
                if (body.healthComponent)
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
            }
            //and our hits burn
            public void OnDamageDealtServer(DamageReport report)
            {
                if (report.damageInfo.procCoefficient > 0)
                {
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = body.gameObject,
                        victimObject = report.victim.gameObject,
                        dotIndex = DotController.DotIndex.Burn,
                        duration = report.damageInfo.procCoefficient * 4f * stack,
                        damageMultiplier = 1
                    };
                    DotController.InflictDot(ref dotInfo);
                }
            }
            private void OnDestroy()
            {
                //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                if (body.healthComponent)
                {
                    int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
                }
            }
        }
    }
}
