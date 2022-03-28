using RoR2;

namespace Moonstorm.Starstorm2.Equipments
{
    [DisabledContent]
    public sealed class AffixVoid : EliteEquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("AffixVoid");

        public override MSEliteDef EliteDef { get; } = SS2Assets.LoadAsset<MSEliteDef>("Void");

        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
