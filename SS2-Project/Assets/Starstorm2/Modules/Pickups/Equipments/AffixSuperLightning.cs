using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
namespace SS2.Equipments
{
    public class AffixSuperLightning : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acSuperLightning", SS2Bundle.Equipments);
        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(SS2Assets.LoadAsset<BuffDef>("BuffAffixSuperLightning", SS2Bundle.Equipments), SS2Assets.LoadAsset<Material>("matSuperLightningOverlay", SS2Bundle.Equipments));
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
            body.RemoveBuff(RoR2Content.Buffs.AffixBlue);
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddBuff(RoR2Content.Buffs.AffixBlue);
        }

        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffAffixSuperLightning;
           
        }
    }
}
