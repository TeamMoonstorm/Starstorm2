using Moonstorm.Components;
using R2API;
using RoR2;
using System;
using UnityEngine;

using Moonstorm;
namespace SS2.Buffs
{
    public sealed class BuffTerminationCooldown : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffTerminationCooldown", SS2Bundle.Items);

        //public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matTerminationOverlay");
    }
}
