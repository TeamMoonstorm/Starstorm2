using Moonstorm.Components;
using SS2.Items;
using R2API;
using RoR2;
using UnityEngine;

using Moonstorm;
namespace SS2.Buffs
{
    public sealed class NemCapManaReduction : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdNemCapManaReduction", SS2Bundle.Indev);
    }
}
