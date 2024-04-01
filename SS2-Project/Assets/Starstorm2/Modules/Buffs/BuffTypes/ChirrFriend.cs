using RoR2;
using UnityEngine;
namespace SS2.Buffs
{
    //[DisabledContent]

    // FIVE FUCKING BUFFS
    // this one is for visuals/icon only
    public sealed class ChirrFriend : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrFriend", SS2Bundle.Chirr);

        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matFriendOverlay", SS2Bundle.Chirr);


    }
}
