using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
namespace SS2.Equipments
{
    // idk how to make this depend on DLC1
    public class AffixSuperEarth : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acSuperEarth", SS2Bundle.Equipments);
        public override void Initialize()
        {
            BuffOverlays.AddBuffOverlay(SS2Content.Buffs.BuffAffixSuperEarth, SS2Assets.LoadAsset<Material>("matSuperEarthOverlay", SS2Bundle.Equipments));
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
            body.RemoveBuff(DLC1Content.Buffs.EliteEarth);
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
            body.AddBuff(DLC1Content.Buffs.EliteEarth);
        }

        public sealed class Behavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffAffixSuperEarth;
           
        }
    }
}
