using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Equipments
{
    public sealed class WhiteFlag : SS2Equipment
    {
        private const string token = "SS2_EQUIP_WHITEFLAG_DESC";
        public override SS2AssetRequest<EquipmentAssetCollection> AssetRequest<EquipmentAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acWhiteFlag", SS2Bundle.Equipments);
        }
        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            _flagObject = assetCollection.FindAsset<GameObject>("WhiteFlagWard");
        }
        private GameObject _flagObject;


        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius of the White Flag's effect, in meters.")]
        [FormatToken(token, 0)]
        public static float flagRadius = 25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of White Flag when used, in seconds.")]
        [FormatToken(token, 1)]
        public static float flagDuration = 8f;

        public override bool Execute(EquipmentSlot slot)
        {            //To do: make better placement system
            GameObject gameObject = Object.Instantiate(_flagObject, slot.characterBody.corePosition, Quaternion.identity);
            BuffWard buffWard = gameObject.GetComponent<BuffWard>();
            buffWard.expireDuration = flagDuration;
            buffWard.radius = flagRadius;
            gameObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;

            return true;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}
