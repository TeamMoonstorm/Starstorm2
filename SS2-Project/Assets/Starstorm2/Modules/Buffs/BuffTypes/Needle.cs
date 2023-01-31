using Moonstorm.Components;
using RoR2;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Needle : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffNeedle", SS2Bundle.Items);

        public sealed class Behavior : BaseBuffBodyBehavior, RoR2.IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffNeedle;
            private void Start()
            {
                if (body.healthComponent)
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
            }
            public void OnIncomingDamageServer(DamageInfo info)
            {
                info.crit = true;
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