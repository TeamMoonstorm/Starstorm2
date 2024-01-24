using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;
using UnityEngine.Networking;
namespace Moonstorm.Starstorm2.Items
{
    public sealed class CoffeeBag : ItemBase
    {
        public const string token = "SS2_ITEM_COFFEEBAG_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("CoffeeBag", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Chance on hit to drop a coffee bean. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float procChance = .08f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Attack speed bonus granted per stack while the buff is active. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float atkSpeedBonus = .08f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Movement speed bonus granted per stack while the buff is active. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float moveSpeedBonus = .08f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of the buff gained upon picking up a coffee bean, per stack.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float buffDuration = 5;

        public static GameObject coffeeBeanPrefab = SS2Assets.LoadAsset<GameObject>("CoffeeBeanPickup", SS2Bundle.Items);
        override public void Initialize()
        {
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
        }

        private void OnServerDamageDealt(DamageReport report)
        {
            int stack = report.attackerBody && report.attackerBody.inventory ? report.attackerBody.inventory.GetItemCount(ItemDef) : 0;

            if (stack > 0 && Util.CheckRoll(procChance * report.damageInfo.procCoefficient * 100f, report.attackerMaster))
            {
                GameObject bean = UnityEngine.Object.Instantiate<GameObject>(coffeeBeanPrefab, report.damageInfo.position, UnityEngine.Random.rotation);
                TeamFilter teamFilter = bean.GetComponent<TeamFilter>();
                if (teamFilter)
                {
                    teamFilter.teamIndex = report.attackerTeamIndex;
                }
                NetworkServer.Spawn(bean);
            }
        }
    }
}
