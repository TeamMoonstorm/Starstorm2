using RoR2;
using RoR2.Items;
using System;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class GreenChocolate : ItemBase
    {
        private const string token = "SS2_ITEM_GREENCHOCOLATE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("GreenChocolate");

        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float damageThreshold = 0.2f;

        [TokenModifier(token, StatTypes.Percentage, 1)]
        public static float damageReduction = 0.5f;

        [TokenModifier(token, StatTypes.Default, 2)]
        public static float baseDuration = 15f;

        [TokenModifier(token, StatTypes.Default, 3)]
        public static float stackDuration = 10f;

        [TokenModifier(token, StatTypes.Percentage, 4)]
        public static float buffDamage = 0.5f;

        [TokenModifier(token, StatTypes.Default, 5)]
        public static float buffCrit = 20f;
        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.GreenChocolate;
            public void Start()
            {
                if (body.healthComponent)
                {
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
                }
            }


            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.damage >= body.healthComponent.fullCombinedHealth * damageThreshold)
                {
                    damageInfo.damage = damageInfo.damage * (1 - damageReduction) + (body.healthComponent.fullCombinedHealth * (damageThreshold * damageReduction));
                    body.AddTimedBuff(SS2Content.Buffs.BuffChocolate, baseDuration + (stackDuration * (stack - 1)));
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
