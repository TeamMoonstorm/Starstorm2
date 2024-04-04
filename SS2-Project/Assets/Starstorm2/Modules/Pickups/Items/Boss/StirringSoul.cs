using SS2.Components;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System.Collections;
using MSU.Config;
using RoR2.ContentManagement;

namespace SS2.Items
{
    public sealed class StirringSoul : SS2Item
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
        private static GameObject _monsterSoulPickup;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Chance to gain soul initially. (1 = 100%)")]
        [FormatToken("SS2_ITEM_STIRRINGSOUL_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float initChance = 0.005f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Soul gain chance cap. (1 = 100%)")]
        public static float maxChance = 0.1f;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<ItemDef>("StirringSoul", SS2Bundle.Items);
            helper.AddAssetToLoad<GameObject>("MonsterSoul", SS2Bundle.Items);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _itemDef = helper.GetLoadedAsset<ItemDef>("StirringSoul");
            _monsterSoulPickup = helper.GetLoadedAsset<GameObject>("MonsterSoul");
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.StirringSoul;
            public float currentChance;

            public void OnKilledOtherServer(DamageReport report)
            {
                if (NetworkServer.active && !Run.instance.isRunStopwatchPaused && report.victimMaster)
                {
                    GameObject soul = Instantiate(_monsterSoulPickup, report.victimBody.corePosition, Random.rotation);
                    soul.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                    SoulPickup pickup = soul.GetComponentInChildren<SoulPickup>();
                    pickup.team = soul.GetComponent<TeamFilter>();
                    pickup.chance = currentChance;
                    pickup.Behavior = this;
                    NetworkServer.Spawn(soul);
                }

            }
            public void ChangeChance(bool reset)
            {
                if (reset)
                {
                    currentChance = initChance * 200;
                }
                else if (currentChance < (maxChance * 200))
                {
                    currentChance += initChance * 200;
                }
            }
        }
    }
}
