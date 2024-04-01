using static R2API.DamageAPI;
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
