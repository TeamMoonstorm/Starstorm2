using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]

    public sealed class BloodTester : ItemBase
    {
        private const string token = "SS2_ITEM_BLOODTESTER_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BloodTester", SS2Bundle.Items);

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of healing required per Blood Tester proc.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float healIncrement = 15f;

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of gold given per Blood Tester proc. DOES NOT SCALE WITH LEVEL.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float goldIncrement = 2f;

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.BloodTester;
            //For graphs of values https://www.desmos.com/calculator/c3bqneyo79
            private float healAccum;
            private float stopwatch;
            private int totalCount;
            private int burstCount;
            private static float goldStagger = 0.5f;        //used to limit how fast gold can be gained

            /*private float healthCoefficient
            {
                get
                {
                    return 1f - 1f / (2f + stack);
                }
            }*/

            public void Start()
            {
                HealthComponent.onCharacterHealServer += AccumulateHeals;
            }

            private void AccumulateHeals(HealthComponent healthComponent, float healAmount, ProcChainMask procChainMask)
            {
                //stop giving money if we're waiting to teleport (prevents a softlock)
                //if there is no teleporter or the teleporter isnt charge, go ahead
                bool flag = TeleporterInteraction.instance ? !TeleporterInteraction.instance.isCharged : true;
                if (!healthComponent.body.Equals(body) || Run.instance.isRunStopwatchPaused || !flag)
                    return;

                if (healthComponent.health < healthComponent.body.maxHealth)
                {
                    healAccum += healAmount;
                }
                healAccum = Mathf.Min(healAccum, healthComponent.fullHealth);
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;
                if (stopwatch > goldStagger)
                {
                    if (healAccum >= healIncrement)
                    {
                        totalCount = Mathf.FloorToInt(healAccum / Mathf.Max(healIncrement, 1f));
                        burstCount = (int)Mathf.Min(totalCount * goldIncrement * stack, Run.instance.GetDifficultyScaledCost((int)goldIncrement * 2));
                        healAccum -= healIncrement * burstCount;
                        body.master.GiveMoney((uint)(goldIncrement * burstCount));
                        //sound/vfx here
                        stopwatch = 0f;
                    }
                }
                else
                    stopwatch += stopwatch += Time.fixedDeltaTime;
            }
            private void OnDestroy()
            {
                HealthComponent.onCharacterHealServer -= AccumulateHeals;
            }
        }
    }
}
