using RoR2;

namespace Moonstorm.Starstorm2.Equipments
{
    public sealed class DivineRight : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("equipDivineRight", SS2Bundle.Indev);

        public override bool FireAction(EquipmentSlot slot)
        {
            //TO-DO:
            //prefab: should probably be a component that parents to body like microbots rather than an item display.
            //reasoning for above being to ensure compatability with every character without setup
            
            //code: make prefab run an entitystate for the swing animation and just play a blast attack from body..?
            //is it possible to run an actual melee attack off of the prefab i wonder?
            //dreading this.
            return true;
        }
    }

}
