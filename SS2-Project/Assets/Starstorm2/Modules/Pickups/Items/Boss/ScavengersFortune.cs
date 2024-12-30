using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace SS2.Items
{
    public sealed class ScavengersFortune : SS2Item, IContentPackModifier
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acScavengersFortune", SS2Bundle.Items);

        private static Xoroshiro128Plus rng;
        public override void Initialize()
        {
            ExplicitPickupDropTable table = SS2Assets.LoadAsset<ExplicitPickupDropTable>("dtBossScavenger", SS2Bundle.Items);
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            Stage.onStageStartGlobal += (stage) =>
            {
                if(NetworkServer.active)
                    rng = new Xoroshiro128Plus(Run.instance.bossRewardRng.nextUlong);
            };
            On.EntityStates.ScavBackpack.Opening.FixedUpdate += Opening_FixedUpdate;
        }

        private void Opening_FixedUpdate(On.EntityStates.ScavBackpack.Opening.orig_FixedUpdate orig, EntityStates.ScavBackpack.Opening self)
        {
            if (NetworkServer.active && self.itemDropCount == 0 && self.itemDropAge > (self.timeBetweenDrops - self.GetDeltaTime())) // should only fire once  nomatter what
            {
                if(rng.nextNormalizedFloat <= .5f)
                {
                    self.itemDropCount++;
                    self.itemDropAge -= self.timeBetweenDrops;
                    self.itemDropAge += self.GetDeltaTime();///
                    self.chestBehavior.dropPickup = PickupCatalog.FindPickupIndex(SS2Content.Items.ScavengersFortune.itemIndex);
                    self.chestBehavior.ItemDrop();
                }             
            }
            orig(self);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(SS2Content.Buffs.BuffWealth))
            {
                args.healthMultAdd += 0.5f;
                args.damageMultAdd += 0.5f;
            }           
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ScavengersFortune;

            public int goldThreshold = 200; //Amount of gold needed to proc
            public int goldAccum = 0; //Amount of gold earned
            public int updateMoney;

            public void FixedUpdate()
            {
                int currentMoney = (int)body.master.money;
                int moneyMade = (currentMoney - updateMoney);
                if (currentMoney > updateMoney)
                {
                    goldAccum += moneyMade;
                }
                int threshold = Run.instance.GetDifficultyScaledCost(goldThreshold);
                if (goldAccum >= threshold)
                {
                    //to-do: make unique Scav buff
                    body.AddTimedBuff(SS2Content.Buffs.BuffWealth, (30f * stack), 1);
                    goldAccum -= threshold;
                }
                updateMoney = currentMoney;
            }
        }
    }
}
