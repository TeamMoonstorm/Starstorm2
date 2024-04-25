using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class CoffeeBag : SS2Item, IContentPackModifier
    {
        public const string token = "SS2_ITEM_COFFEEBAG_DESC";
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
        private BuffDef _buffDef;
        private GameObject _coffeeBean;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Chance on hit to drop a coffee bean. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float procChance = .08f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Attack speed bonus granted per stack while the buff is active. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float atkSpeedBonus = .08f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Movement speed bonus granted per stack while the buff is active. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float moveSpeedBonus = .08f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of the buff gained upon picking up a coffee bean, per stack.")]
        [FormatToken(token, 3)]
        public static float buffDuration = 5;

        public override void Initialize()
        {
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            RecalculateStatsAPI.GetStatCoefficients += CalculateStatsCoffeeBag;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<BuffDef>("BuffCoffeeBag", SS2Bundle.Items);
            helper.AddAssetToLoad<ItemDef>("CoffeeBag", SS2Bundle.Items);
            helper.AddAssetToLoad<GameObject>("CoffeeBeanPickup", SS2Bundle.Items);

            helper.Start();
            while (!helper.IsDone()) yield return null;

            _buffDef = helper.GetLoadedAsset<BuffDef>("BuffCoffeeBag");
            _itemDef = helper.GetLoadedAsset<ItemDef>("CoffeeBag");
            _coffeeBean = helper.GetLoadedAsset<GameObject>("CoffeeBeanPickup");
        }

        private void CalculateStatsCoffeeBag(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffStack = sender.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag);
            if (buffStack > 0)
            {
                args.attackSpeedMultAdd += atkSpeedBonus * buffStack;
                args.moveSpeedMultAdd += moveSpeedBonus * buffStack;
            }
        }

        private void OnServerDamageDealt(DamageReport report)
        {
            int stack = report.attackerBody && report.attackerBody.inventory ? report.attackerBody.inventory.GetItemCount(ItemDef) : 0;

            if (stack > 0 && Util.CheckRoll(procChance * report.damageInfo.procCoefficient * 100f, report.attackerMaster))
            {
                GameObject bean = UnityEngine.Object.Instantiate<GameObject>(_coffeeBean, report.damageInfo.position, UnityEngine.Random.rotation);
                TeamFilter teamFilter = bean.GetComponent<TeamFilter>();
                if (teamFilter)
                {
                    teamFilter.teamIndex = report.attackerTeamIndex;
                }
                NetworkServer.Spawn(bean);
            }
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_buffDef);
        }
    }
}
