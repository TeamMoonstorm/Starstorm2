using R2API;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.DamageTypes
{
    public sealed class ErraticGadget : DamageTypeBase
    {
        public override DamageAPI.ModdedDamageType ModdedDamageType { get; protected set; }

        public static ModdedDamageType erraticDamageType;

        public override void Initialize()
        {
            erraticDamageType = ModdedDamageType;
        }

        public override void Delegates()
        {
        }
    }
}
