using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Items
{
#if DEBUG
    public sealed class ScavengersFortune : SS2Item, IContentPackModifier
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acScavengersFortune", SS2Bundle.Items);

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.healthMultAdd += 0.5f;
            args.damageMultAdd += 0.5f;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
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
