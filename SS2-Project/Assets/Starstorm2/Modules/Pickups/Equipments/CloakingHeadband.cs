using RoR2;

namespace Moonstorm.Starstorm2.Equipments
{
    public sealed class CloakingHeadband : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("CloakingHeadband");

        [ConfigurableField(ConfigDesc = "How long the Cloak buff lasts, in seconds.")]
        [TokenModifier("SS2_EQUIP_CLOAKINGHEADBAND_DESC", StatTypes.Default, 0)]
        public static float cloakDuration = 8f;
        public override bool FireAction(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak.buffIndex, cloakDuration);
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.CloakSpeed.buffIndex, cloakDuration);
            return true;
        }
    }

}
