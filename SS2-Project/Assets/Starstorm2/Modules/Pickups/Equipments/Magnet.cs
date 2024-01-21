using RoR2;

namespace Moonstorm.Starstorm2.Equipments
{
    public sealed class Magnet : EquipmentBase
    {
        private const string token = "SS2_EQUIP_MAGNET_DESC";
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("Magnet", SS2Bundle.Equipments);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Range at which Simple Magnet can pull pickups, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float magnetRadius = 90f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Percent chance for Simple Magnet to unearth treasure.")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float treasureChance = 10f;

        public override bool FireAction(EquipmentSlot slot)
        {
            return false; /////////
        }
    }
}