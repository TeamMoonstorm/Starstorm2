using R2API;
using RoR2;
using UnityEngine;
using static R2API.DamageAPI;

using Moonstorm;
namespace SS2.DamageTypes
{
    public sealed class RedirectHologram : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get; protected set; }

        public static ModdedDamageType damageType;

        public override void Initialize()
        {
            damageType = ModdedDamageType;
        }

        
        public override void Delegates()
        {
        }
    }
}
