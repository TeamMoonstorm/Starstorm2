using RoR2;
using System.Collections.Generic;
using UnityEngine;

using Moonstorm;
namespace SS2.Equipments
{
    //[DisabledContent]
    public sealed class AffixEthereal : EliteEquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("AffixEthereal", SS2Bundle.Equipments);

        public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        {
            SS2Assets.LoadAsset<MSEliteDef>("edEthereal", SS2Bundle.Equipments),
        };

        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
