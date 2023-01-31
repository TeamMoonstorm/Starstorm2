using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class NemCommandoLol : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("NemCommandoLol", SS2Bundle.Nemmando);

    }
}
