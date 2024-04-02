using RoR2;
namespace SS2.Equipments
{
    public sealed class CloakingHeadband : SS2Equipment
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("CloakingHeadband", SS2Bundle.Equipments);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "How long the Cloak buff lasts, in seconds.")]
        [TokenModifier("SS2_EQUIP_CLOAKINGHEADBAND_DESC", StatTypes.Default, 0)]
        public static float cloakDuration = 8f;
        public override bool FireAction(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak.buffIndex, cloakDuration);
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.CloakSpeed.buffIndex, cloakDuration);
            EffectData effectData = new EffectData
            {
                origin = slot.characterBody.transform.position
            };
            effectData.SetNetworkedObjectReference(slot.characterBody.gameObject);
            EffectManager.SpawnEffect(EntityStates.Bandit2.StealthMode.smokeBombEffectPrefab, effectData, transmit: true);
            return true;
        }
    }

}
