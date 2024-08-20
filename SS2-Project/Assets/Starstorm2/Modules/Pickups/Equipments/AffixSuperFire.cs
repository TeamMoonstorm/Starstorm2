using MSU;
using RoR2;
using RoR2.ContentManagement;

namespace SS2.Equipments
{
    public class AffixSuperFire : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acSuperFire", SS2Bundle.Equipments);
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
            body.RemoveBuff(RoR2Content.Buffs.AffixRed);
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddBuff(RoR2Content.Buffs.AffixRed);
        }

        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffAffixSuperFire;
           
        }
    }
}
