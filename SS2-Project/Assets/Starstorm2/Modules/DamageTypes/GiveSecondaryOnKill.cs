using R2API;
using static R2API.DamageAPI;

namespace Moonstorm.Starstorm2.DamageTypes
{
    public sealed class GiveSecondaryOnKill : DamageTypeBase
    {
        public override DamageAPI.ModdedDamageType ModdedDamageType { get; protected set; }

        public static ModdedDamageType giveSecondaryOnKill;

        public override void Initialize()
        {
            giveSecondaryOnKill = ModdedDamageType;
        }

        public override void Delegates()
        {
        }
    }
}
