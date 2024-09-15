using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Equipments
{
    public sealed class CloakingHeadband : SS2Equipment
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acCloakingHeadband", SS2Bundle.Equipments);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "How long the Cloak buff lasts, in seconds.")]
        [FormatToken("SS2_EQUIP_CLOAKINGHEADBAND_DESC")]
        public static float cloakDuration = 16f;

        public override bool Execute(EquipmentSlot slot)
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
