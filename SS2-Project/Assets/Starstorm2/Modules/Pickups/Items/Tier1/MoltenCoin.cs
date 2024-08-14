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
    public sealed class MoltenCoin : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acMoltenCoin", SS2Bundle.Items);
        private static GameObject _impactEffect;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Chance for Molten Coin to Proc. (100 = 100%)")]
        [FormatToken("SS2_ITEM_MOLTENCOIN_DESC", 0)]
        public static float procChance = 6f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base Damage per stack. (1 = 100%)")]
        [FormatToken("SS2_ITEM_MOLTENCOIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageCoeff = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Coin gain on proc. Scales with time. (1 = 1$)")]
        [FormatToken("SS2_ITEM_MOLTENCOIN_DESC", 2)]
        public static int coinGain = 1;

        public override void Initialize()
        {
            _impactEffect = AssetCollection.FindAsset<GameObject>("MoltenCoinEffect");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.MoltenCoin;
            public void OnDamageDealtServer(DamageReport report)
            {
                if (Util.CheckRoll(procChance * report.damageInfo.procCoefficient, body.master))
                {
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = body.gameObject,
                        victimObject = report.victim.gameObject,
                        dotIndex = DotController.DotIndex.Burn,
                        duration = report.damageInfo.procCoefficient * 4f,
                        damageMultiplier = stack * damageCoeff
                    };
                    StrengthenBurnUtils.CheckDotForUpgrade(report.attackerBody.inventory, ref dotInfo);
                    DotController.InflictDot(ref dotInfo);

                    body.master.GiveMoney((uint)(stack * (Run.instance.stageClearCount + (coinGain * 1f))));

                    EffectManager.SimpleEffect(MoltenCoin._impactEffect, report.victimBody.transform.position, Quaternion.identity, true);
                    EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, report.victimBody.transform.position, UnityEngine.Vector3.up, true);
                }
            }
        }
    }
}
