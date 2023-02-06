using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class ScavengersFortune : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("ScavengersFortune", SS2Bundle.Indev);

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ScavengersFortune;

            public float goldThreshold = 20000f; //Amount of gold needed to proc
            public float goldAccum = 0f; //Amount of gold earned
            public float updateMoney;

            public void FixedUpdate()
            {
                float currentMoney = body.master.money;
                float moneyMade = (currentMoney - updateMoney); //These floats are named like shit - fuck you!

                if (currentMoney > updateMoney)
                {
                    goldAccum += moneyMade;
                    //Debug.Log("goldAccum = " + goldAccum + " moneyMade = " + moneyMade);
                    //There's probably a better way to code all of this. Not sure how, can't be bothered.
                }
                if (goldAccum >= goldThreshold)
                {
                    //to-do: make unique Scav buff
                    body.AddTimedBuff(SS2Content.Buffs.BuffWealth, (30f * stack), 1);
                    goldAccum -= goldThreshold;
                }
                updateMoney = currentMoney;
            }
        }
    }
}
