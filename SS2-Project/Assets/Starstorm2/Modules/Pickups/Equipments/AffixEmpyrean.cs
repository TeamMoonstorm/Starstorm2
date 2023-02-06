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
            public void Start()
            {
                //body.transform.localScale *= 0.65f;
                originalScale = transform.localScale;
                transform.localScale = new Vector3(1.35f * transform.localScale.x, 1.35f * transform.localScale.y, 1.35f * transform.localScale.z);
            }

            public void OnDestroy()
            {
                //body.transform.localScale *= 1.35f;
                transform.localScale = originalScale;
            }
        }
    }
}
