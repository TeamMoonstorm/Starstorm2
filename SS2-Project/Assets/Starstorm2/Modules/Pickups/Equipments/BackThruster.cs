using RoR2;

namespace Moonstorm.Starstorm2.Equipments
{
    public sealed class BackThruster : EquipmentBase
    {
        private const string token = "SS2_EQUIP_BACKTHRUSTER_DESC";
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("BackThruster", SS2Bundle.Equipments);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "How long the Thruster buff lasts, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float thrustDuration = 8f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Maximum speed bonus from Thruster (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float speedCap = 2f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "How long it takes to reach maximum speed, in seconds")]
        public static float accel = 1.5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Maximum turning angle before losing built up speed")]
        public static float maxAngle = 15f;
        public override bool FireAction(EquipmentSlot slot)
        {
            var characterMotor = slot.characterBody.characterMotor;
            if (characterMotor)
            {
                slot.characterBody.AddTimedBuff(SS2Content.Buffs.BuffBackThruster, thrustDuration);
                return true;
            }
            return false;
        }
    }
}