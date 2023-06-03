using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class StirringSoul : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("StirringSoul", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Chance to gain soul initially. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_STIRRINGSOUL_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float initChance = 0.005f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Soul gain chance cap. (1 = 100%)")]
        public static float maxChance = 0.1f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.StirringSoul;
            public float currentChance;
            private static GameObject MonsterSoulPickup = SS2Assets.LoadAsset<GameObject>("MonsterSoul", SS2Bundle.Items);
            public void OnKilledOtherServer(DamageReport report)
            {
                if (NetworkServer.active && !Run.instance.isRunStopwatchPaused && report.victimMaster)
                {
                    GameObject soul = Instantiate(MonsterSoulPickup, report.victimBody.corePosition, Random.rotation);
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
