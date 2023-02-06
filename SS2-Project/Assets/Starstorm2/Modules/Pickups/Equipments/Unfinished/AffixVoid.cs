using RoR2;
using System.Collections.Generic;

namespace Moonstorm.Starstorm2.Equipments
{
    public sealed class AffixVoid : EliteEquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("AffixVoid", SS2Bundle.Indev);

        public override List<MSEliteDef> EliteDefs => new List<MSEliteDef>
        {
            SS2Assets.LoadAsset<MSEliteDef>("Void", SS2Bundle.Indev)
        };

        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
