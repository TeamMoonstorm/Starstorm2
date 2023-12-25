using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using System;
using UnityEngine;
using RoR2.CharacterAI;
using System.Linq;

namespace Moonstorm.Starstorm2.Buffs
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
