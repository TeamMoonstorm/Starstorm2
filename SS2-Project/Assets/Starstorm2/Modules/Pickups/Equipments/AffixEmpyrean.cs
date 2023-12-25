using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Starstorm2.Equipments
{
    //[DisabledContent]
    public sealed class AffixEmpyrean : EliteEquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("AffixEmpyrean", SS2Bundle.Equipments);

        public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        {
            SS2Assets.LoadAsset<MSEliteDef>("edEmpyrean", SS2Bundle.Equipments),
        };

        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
