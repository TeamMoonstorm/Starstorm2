using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Items
{
#if DEBUG
    public sealed class ScavengersFortune : SS2Item
    {
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "ScavengersFortune" - Indev
             */
            yield break;
        }

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
#endif
}
