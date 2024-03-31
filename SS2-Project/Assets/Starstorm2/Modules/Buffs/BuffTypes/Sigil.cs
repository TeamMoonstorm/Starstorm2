using Moonstorm.Components;
using SS2.Items;
using R2API;
using RoR2;
using UnityEngine;

using Moonstorm;
namespace SS2.Buffs
{
    public sealed class Sigil : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffSigil", SS2Bundle.Items);
        public override Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matSigilBuffOverlay", SS2Bundle.Items);

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSigil;
            public void OnDestroy()
            {
                body.SetBuffCount(SS2Content.Buffs.BuffSigilHidden.buffIndex, 0);
            }
        }
    }
}
