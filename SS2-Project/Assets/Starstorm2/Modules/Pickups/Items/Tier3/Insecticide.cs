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
    public sealed class Insecticide : SS2Item
    {
        public override SS2AssetRequest<ItemAssetCollection> AssetRequest<ItemAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acInsecticide", SS2Bundle.Items);
        }
        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            _hitEffect = assetCollection.FindAsset<GameObject>("InsecticideEffect");
        }

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Chance. (1 = 100%)")]
        [FormatToken("SS2_ITEM_INSECTICIDE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float chance = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Total damage. (1 = 100%)")]
        [FormatToken("SS2_ITEM_INSECTICIDE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damageCoeff = 1.8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of poison.")]
        public static float duration = 3;

        private static GameObject _hitEffect;

        public static DotController.DotIndex DotIndex { get; private set; }
        public override void Initialize()
        {
            DotController.onDotInflictedServerGlobal += RefreshInsects;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "Insecticide" - Items
             * GameObject - "InsecticideEffect" - Items
             */
            yield break;
        }

        //Remove this as DotAPI can handle refreshment.
        private void RefreshInsects(DotController dotController, ref InflictDotInfo inflictDotInfo)
        {
            if (inflictDotInfo.dotIndex == DotIndex)
            {
                int i = 0;
                int count = dotController.dotStackList.Count;

                while (i < count)
                {
                    if (dotController.dotStackList[i].dotIndex == DotIndex)
                    {
                        dotController.dotStackList[i].timer = Mathf.Max(dotController.dotStackList[i].timer, duration);
                    }
                    i++;
                }
            }
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Insecticide;
            public void OnDamageDealtServer(DamageReport report)
            {
                if (report.victimBody.teamComponent.teamIndex != report.attackerBody.teamComponent.teamIndex && report.damageInfo.procCoefficient > 0 && Util.CheckRoll(chance * 100, report.attackerMaster))
                {
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = body.gameObject,
                        victimObject = report.victim.gameObject,
                        dotIndex = DotIndex,
                        duration = report.damageInfo.procCoefficient * duration,
                        damageMultiplier = stack * (damageCoeff / 1.8f)
                    };
                    DotController.InflictDot(ref dotInfo);

                    // GOOPY SOUNDS HERE WOULD BE FANTASTIC!!!!!!!!!!!!!!!!!!!!!!!!!!
                    EffectManager.SimpleEffect(_hitEffect, report.damageInfo.position, Quaternion.identity, true);
                }
            }
        }
    }
}
