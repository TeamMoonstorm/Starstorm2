using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Sigil : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffSigil", SS2Bundle.Items);
        public override Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matSigilBuffOverlay", SS2Bundle.Items);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSigil;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //the stacking amounts are added by the item - these base values are here in case the buff is granted by something other than sigil
                args.armorAdd += HuntersSigil.baseArmor;
                args.damageMultAdd += HuntersSigil.baseDamage;
            }
        }
    }
}
