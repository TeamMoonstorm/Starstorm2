using RoR2;
using RoR2.Items;
namespace SS2.Items
{
    public sealed class BloodTester : SS2Item
    {
        private const string token = "SS2_ITEM_BLOODTESTER_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BloodTester", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of health regeneration granted per 25 gold, per stack. (1 = 1 hp/s)")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float healthRegen = 0.4f;

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            int stack = sender.inventory?.GetItemCount(SS2Content.Items.BloodTester) ?? 0;
            if (stack > 0)
                args.baseRegenAdd += (healthRegen * stack) * sender.master.money / Run.instance.GetDifficultyScaledCost(25);
        }

        // need to force recalculatestats to make regen update with money
        // not ideal, but shouldnt be a performance hit. only really runs when killing an enemy or using an interactable
        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.BloodTester;
            private uint moneyLastFrame;

            private void FixedUpdate()
            {
                if (!body.master) return;
                if (moneyLastFrame != body.master.money)
                    body.RecalculateStats();
                moneyLastFrame = body.master.money;
            }
        }
    }
}
