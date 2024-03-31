using R2API;
using static R2API.DamageAPI;

using Moonstorm;
namespace SS2.DamageTypes
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
