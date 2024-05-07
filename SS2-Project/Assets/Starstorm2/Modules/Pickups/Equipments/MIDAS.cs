using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Equipments
{
    public sealed class MIDAS : SS2Equipment
    {
        public override SS2AssetRequest<EquipmentAssetCollection> AssetRequest<EquipmentAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acMIDAS", SS2Bundle.Equipments);
        }

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Health percentage sacrificed (1 = 100%)")]
        [FormatToken("SS2_EQUIP_MIDAS_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float healthPercentage = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM)]
        public static float goldMultiplier = 1f;

        public override bool Execute(EquipmentSlot slot)
        {
            int playerCount = 0;
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                playerCount++;
            }

            var level = slot.characterBody.level;

            float commandoHealth = .5f * (110 + (33 * (level - 1)));
            float healthLost = slot.characterBody.healthComponent.health * healthPercentage;
            float chestFraction = healthLost / commandoHealth;

            var goldEarned = Run.instance.GetDifficultyScaledCost((int)(25 * chestFraction));

            goldEarned = Mathf.CeilToInt(Mathf.Max(goldEarned, healthLost));
            DamageInfo damageInfo = new DamageInfo()
            {
                damage = healthLost,
                damageType = DamageType.BypassArmor,
                damageColorIndex = DamageColorIndex.Item,
                inflictor = slot.characterBody.gameObject,
                position = slot.characterBody.healthComponent.gameObject.transform.position,
            };
            slot.characterBody.healthComponent.TakeDamage(damageInfo);
            if (!slot.characterBody.isPlayerControlled && slot.characterBody.teamComponent.teamIndex == TeamIndex.Player)
            {
                uint splitAmount = (uint)((goldEarned * goldMultiplier) / playerCount);
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    player.master.GiveMoney(splitAmount);
                }
            }
            else
            {
                slot.characterBody.master.GiveMoney((uint)goldEarned);
            }

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
