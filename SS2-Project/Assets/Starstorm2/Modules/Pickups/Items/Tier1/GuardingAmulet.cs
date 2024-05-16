using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class GuardingAmulet : SS2Item
    {
        public const string token = "SS2_ITEM_GUARDINGAMULET_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acGuardingAmulet", SS2Bundle.Items);
        private static GameObject _shieldEffect;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Damage reduction per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float damageReduction = 0.3f;

        public override void Initialize()
        {
            _shieldEffect = AssetCollection.FindAsset<GameObject>("AmuletShieldEffect");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.GuardingAmulet;

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (!damageInfo.attacker) return;

                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                Vector3 attackerPosition = attackerBody ? attackerBody.corePosition : damageInfo.attacker.transform.position;
                Vector3 between = damageInfo.position - attackerPosition;
                if (BackstabManager.IsBackstab(between, this.body))
                {
                    float reduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(damageReduction * this.stack * 100f) / 100f;
                    damageInfo.damage *= 1 - reduction;

                    EffectData effectData = new EffectData
                    {
                        origin = this.body.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(attackerPosition - damageInfo.position),
                        scale = this.body.radius,
                    };
                    effectData.SetNetworkedObjectReference(this.body.gameObject);
                    EffectManager.SpawnEffect(_shieldEffect, effectData, true);
                }

            }
        }
    }
}
