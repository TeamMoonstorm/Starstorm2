using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class BuffEchelon : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffEchelon", SS2Bundle.Indev);

        //public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matTerminationOverlay");

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffEchelon;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //the stacking amounts are added by the item - these base values are here in case the buff is granted by something other than ~~sigil~~ echelon
                args.baseDamageAdd += RelicOfEchelon.damageBonus;
                args.baseHealthAdd += RelicOfEchelon.healthBonus;
            }
        }
    }
}
