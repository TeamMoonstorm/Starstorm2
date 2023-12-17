using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Equipments
{
    //[DisabledContent]
    public sealed class AffixEmpyrean : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("AffixEmpyrean", SS2Bundle.Indev);


        // override MSEliteDef EliteDef { get; set; } = Assets.Instance.MainAssetBundle.LoadAsset<MSEliteDef>("Void");

        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<Behavior>(stack);
        }

        public sealed class Behavior : CharacterBody.ItemBehavior
        {
            private Vector3 originalScale;
        }
    }
}
