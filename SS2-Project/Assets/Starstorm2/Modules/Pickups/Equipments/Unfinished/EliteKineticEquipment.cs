using RoR2;
using System.Collections.Generic;

namespace Moonstorm.Starstorm2.Equipments
{
    [DisabledContent]

    public sealed class EliteKineticEquipment : EliteEquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("EliteKineticEquipment", SS2Bundle.Indev);

        public override List<MSEliteDef> EliteDefs => new List<MSEliteDef>
        {
            SS2Assets.LoadAsset<MSEliteDef>("edKinetic", SS2Bundle.Indev),
            SS2Assets.LoadAsset<MSEliteDef>("edKineticHonor", SS2Bundle.Indev)
        };

        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
