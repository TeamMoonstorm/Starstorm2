using RoR2;
using RoR2.Items;
using UnityEngine;
namespace SS2.Items
{

    public sealed class MoltenCoin : SS2Item
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("MoltenCoin", SS2Bundle.Items);

        public static GameObject impactEffect { get; } = SS2Assets.LoadAsset<GameObject>("MoltenCoinEffect", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Chance for Molten Coin to Proc. (100 = 100%)")]
        [FormatToken("SS2_ITEM_MOLTENCOIN_DESC",   0)]
        public static float procChance = 6f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base Damage per stack. (1 = 100%)")]
        [FormatToken("SS2_ITEM_MOLTENCOIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 1, "100")]
        public static float damageCoeff = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Coin gain on proc. Scales with time. (1 = 1$)")]
        [FormatToken("SS2_ITEM_MOLTENCOIN_DESC",   2)]
        public static int coinGain = 1;

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
                        
                    //MSUtil.PlayNetworkedSFX("MoltenCoin", report.victim.gameObject.transform.position);
                    //moved the sound to an effect so it takes advantage of vfx priority
                    EffectManager.SimpleEffect(MoltenCoin.impactEffect, report.victimBody.transform.position, Quaternion.identity, true);
                    EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, report.victimBody.transform.position, UnityEngine.Vector3.up, true);                  
                }
            }
        }
    }
}
