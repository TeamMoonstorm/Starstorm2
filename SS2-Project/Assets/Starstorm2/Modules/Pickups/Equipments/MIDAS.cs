using RoR2;

namespace Moonstorm.Starstorm2.Equipments
{
    //[DisabledContent]
    public sealed class MIDAS : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("MIDAS");
        public float goldEarned;

        //★ There's probably a way to do this involving an item behavior. Let me know about it.
        //N Nah, this looks good.
        public override bool FireAction(EquipmentSlot slot)
        {
            goldEarned = slot.characterBody.healthComponent.health * 0.5f;
            DamageInfo damageInfo = new DamageInfo()
            {
                damage = slot.characterBody.healthComponent.health * 0.5f,
                damageType = DamageType.BypassArmor,
                damageColorIndex = DamageColorIndex.Item,
                inflictor = slot.characterBody.gameObject,
                position = slot.characterBody.healthComponent.gameObject.transform.position,
            };
            slot.characterBody.healthComponent.TakeDamage(damageInfo);
            slot.characterBody.master.GiveMoney((uint)goldEarned);
            return true;
        }
    }

}
