using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.ExpansionManagement;
namespace SS2
{
    public static class RewardCatalog
    {
        private static RewardDef[] rewardDefs = Array.Empty<RewardDef>();
        private static RewardTierDef[] tierDefs = Array.Empty<RewardTierDef>();

        private static ExpansionDef DLC1; // apparently no way to grab this at runtime w/o loading the asset

        // gonna just be items for now
        // would like to implement unique pickups
        //guaranteed printers next stage
        //guaranteed red next stage
        // insta drones
        // invincible golem friend
        [SystemInitializer(typeof(PickupCatalog))]
        private static void InitRewardTable()
        {
            Run.onRunStartGlobal += RegenerateAll;
            DLC1 = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
            tierDefs = new RewardTierDef[9];
            // TODO: fix this it sucks and errors if items are disabled
            try
            {
                #region Rewards
                //0
                RewardTierDef tier = new RewardTierDef();
                tier.valueBreakpoint = 0;
                tier.Add(new RewardDef { white = 2 });
                tier.Add(new RewardDef { whiteOption = 1 });
                tierDefs[0] = tier;

                //10
                tier = new RewardTierDef();
                tier.valueBreakpoint = 10;
                tier.Add(new RewardDef { white = 3 });
                tier.Add(new RewardDef { white = 1, whiteOption = 1 });
                tier.Add(new RewardDef { whiteCommand = 1 });
                RewardDef shrooms = new RewardDef();
                shrooms.AddPickups(RoR2Content.Items.Mushroom, 4);
                tier.Add(shrooms, 0.5f);
                tierDefs[1] = tier;

                //20
                tier = new RewardTierDef();
                tier.valueBreakpoint = 20;
                tier.Add(new RewardDef { green = 2 });
                tier.Add(new RewardDef { green = 1, whiteOption = 2 });
                tier.Add(new RewardDef { greenOption = 1, white = 1 });
                RewardDef crits = new RewardDef();
                crits.AddPickups(RoR2Content.Items.CritGlasses, 3);
                tier.Add(crits, 0.5f);
                tierDefs[2] = tier;

                //40
                tier = new RewardTierDef();
                tier.valueBreakpoint = 40;
                tier.Add(new RewardDef { green = 3, whiteCommand = 1 });
                tier.Add(new RewardDef { greenOption = 2, whiteOption = 1 });
                tier.Add(new RewardDef { greenCommand = 1, white = 4 });
                tier.Add(new RewardDef { white = 8 });
                tier.Add(new RewardDef { whiteCommand = 4 });
                RewardDef fire = new RewardDef();
                fire.AddPickups(RoR2Content.Items.IgniteOnKill, 2);
                fire.AddPickups(SS2Content.Items.MoltenCoin, 2);
                fire.AddPickups(DLC1Content.Items.StrengthenBurn, 1);
                fire.isAvailable = RequireDLC1;
                tier.Add(fire, 1f);
                tierDefs[3] = tier;

                //70
                tier = new RewardTierDef();
                tier.valueBreakpoint = 70;
                tier.Add(new RewardDef { red = 1, greenOption = 2, whiteOption = 4 });
                tier.Add(new RewardDef { greenCommand = 3, whiteCommand = 6 });
                tier.Add(new RewardDef { greenOption = 8 });
                tier.Add(new RewardDef { green = 6, white = 12 });
                RewardDef deathmark = new RewardDef();
                deathmark.AddPickups(SS2Content.Items.MoltenCoin, 4);
                deathmark.AddPickups(RoR2Content.Items.BleedOnHit, 4);
                deathmark.AddPickups(SS2Content.Items.DetritiveTrematode, 4);
                deathmark.AddPickups(RoR2Content.Items.SlowOnHit, 3);
                deathmark.AddPickups(SS2Content.Items.StrangeCan, 3);
                deathmark.AddPickups(RoR2Content.Items.DeathMark, 1);
                tier.Add(deathmark, 1f);
                tierDefs[4] = tier;


                //100
                tier = new RewardTierDef();
                tier.valueBreakpoint = 100;
                tier.Add(new RewardDef { redOption = 1, green = 2, white = 4 });
                tier.Add(new RewardDef { greenOption = 6, whiteOption = 12 });
                RewardDef reroll = new RewardDef { red = 2 };
                reroll.AddPickups(RoR2Content.Equipment.Recycle, 1);
                tier.Add(reroll, 1f);
                RewardDef missiles = new RewardDef();
                missiles.AddPickups(DLC1Content.Items.MoreMissile, 1);
                missiles.AddPickups(RoR2Content.Equipment.CommandMissile, 1);
                missiles.AddPickups(RoR2Content.Items.Missile, 1);
                missiles.AddPickups(SS2Content.Items.ArmedBackpack, 1);
                missiles.AddPickups(RoR2Content.Items.Firework, 1);
                tier.Add(missiles, .67f);
                tierDefs[5] = tier;

                //150
                tier = new RewardTierDef();
                tier.valueBreakpoint = 150;
                tier.Add(new RewardDef { redCommand = 1, green = 2, white = 4 });
                tier.Add(new RewardDef { redOption = 1, greenCommand = 4, whiteCommand = 8 });
                tier.Add(new RewardDef { red = 2, green = 8, white = 16 });
                RewardDef lightning = new RewardDef();
                lightning.AddPickups(SS2Content.Items.ErraticGadget, 1);
                lightning.AddPickups(RoR2Content.Equipment.Lightning, 1);
                lightning.AddPickups(RoR2Content.Items.ChainLightning, 2);
                lightning.AddPickups(SS2Content.Items.LightningOnKill, 2);
                lightning.AddPickups(RoR2Content.Items.LightningStrikeOnHit, 1);
                tier.Add(lightning, 1f);
                // need "summon drone" pickup
                RewardDef drones = new RewardDef();
                drones.AddPickups(DLC1Content.Items.DroneWeapons, 1);
                drones.AddPickups(SS2Content.Items.CompositeInjector, 2);
                drones.AddPickups(RoR2Content.Equipment.DroneBackup, 3);
                drones.AddPickups(RoR2Content.Items.EquipmentMagazine, 4);
                drones.isAvailable = RequireDLC1;
                tier.Add(drones, 1f);
                tierDefs[6] = tier;

                //200
                tier = new RewardTierDef();
                tier.valueBreakpoint = 200;
                tier.Add(new RewardDef { redCommand = 2, green = 3, white = 6 });
                tier.Add(new RewardDef { redOption = 3, greenOption = 6, whiteOption = 9 });
                RewardDef supercrit = new RewardDef();
                supercrit.AddPickups(RoR2Content.Equipment.CritOnUse, 1);
                supercrit.AddPickups(RoR2Content.Items.CritGlasses, 10);
                supercrit.AddPickups(RoR2Content.Items.AttackSpeedOnCrit, 5);
                supercrit.AddPickups(RoR2Content.Items.HealOnCrit, 5);
                supercrit.AddPickups(RoR2Content.Items.BleedOnHitAndExplode, 1);
                supercrit.AddPickups(DLC1Content.Items.CritDamage, 2);
                supercrit.isAvailable = RequireDLC1;
                tier.Add(supercrit, 1f);
                RewardDef elite = new RewardDef();
                elite.AddPickups(RoR2Content.Items.ExecuteLowHealthElite, 5);
                elite.AddPickups(RoR2Content.Items.KillEliteFrenzy, 2);
                elite.AddPickups(RoR2Content.Items.HeadHunter, 2);
                elite.AddPickups(SS2Content.Items.DroidHead, 2);
                elite.AddPickups(SS2Content.Items.CompositeInjector, 2);
                elite.AddPickups(RoR2Content.Equipment.AffixBlue, 1);
                elite.AddPickups(RoR2Content.Equipment.AffixRed, 1);
                elite.AddPickups(RoR2Content.Equipment.AffixWhite, 1);
                tier.Add(elite, 1f);
                tierDefs[7] = tier;

                //300
                tier = new RewardTierDef();
                tier.valueBreakpoint = 300;
                tier.Add(new RewardDef { redCommand = 5 });
                RewardDef god = new RewardDef();
                god.AddPickups(SS2Content.Items.CompositeInjector, 1);
                god.AddPickups(SS2Content.Equipments.AffixEmpyrean, 1);
                god.AddPickups(RoR2Content.Items.ExtraLife, 5);
                god.AddPickups(RoR2Content.Items.ShinyPearl, 10);
                tier.Add(god);
                tierDefs[8] = tier;
                #endregion
            }
            catch(Exception e)
            {
                SS2Log.Error("Failed to create RewardCatalog.\n\n" + e);
            }

        }

        public static RewardTierDef GetHighestRewardTier(int value)
        {
            RewardTierDef highestTier = null;
            int highestValue = 0;
            foreach(RewardTierDef tier in tierDefs)
            {
                if(tier.valueBreakpoint >= highestValue && tier.valueBreakpoint <= value)
                {
                    highestValue = tier.valueBreakpoint;
                    highestTier = tier;
                }
            }
            if(highestTier == null)
            {
                SS2Log.Error("RewardCatalog.GetHighestRewardTier(int value): Failed to return reward tier.");
                return null;
            }
            return highestTier;
        }

        public static void RegenerateAll(Run run)
        {
            foreach(RewardTierDef tier in tierDefs)
            {
                tier.Regenerate(run);
            }    
        }
        public static bool RequireDLC1(Run run)
        {
            return run.IsExpansionEnabled(DLC1);
        }
        // collection of pickup      
    }
    public class RewardDef
    {
        public PickupIndex[] pickups = Array.Empty<PickupIndex>();

        // ugly. but choice/command drops arent real Pickups so they have to be hardcoded.
        public int white;
        public int whiteOption;
        public int whiteCommand;
        public int green;
        public int greenOption;
        public int greenCommand;
        public int red;
        public int redOption;
        public int redCommand;

        public Func<Run, bool> isAvailable = (Run run) => true;

        public static PickupIndex[] Combine(RewardDef a, RewardDef b)
        {
            return HG.ArrayUtils.Join(a.pickups, b.pickups);
        }

        public void AddPickups(ItemDef def, int count)
        {
            if (def == null)
                return;

            AddPickups(PickupCatalog.FindPickupIndex(def.itemIndex), count);
        }
        public void AddPickups(EquipmentDef def, int count)
        {
            if (def == null)
                return;

            AddPickups(PickupCatalog.FindPickupIndex(def.equipmentIndex), count);
        }
        public void AddPickups(MiscPickupDef def, int count)
        {
            if (def == null)
                return;

            AddPickups(PickupCatalog.FindPickupIndex(def.miscPickupIndex), count);
        }
        public void AddPickups(PickupIndex pickupIndex, int count)
        {
            Array.Resize(ref pickups, pickups.Length + count);
            for (int i = pickups.Length - 1; i > pickups.Length - 1 - count; i--)
            {
                pickups[i] = pickupIndex;
            }
        }
    }
    public class RewardTierDef
    {
        public struct WeightedRewardDef
        {
            public RewardDef rewardDef;
            public float weight;
        }
        public List<WeightedRewardDef> rewardDefs = new List<WeightedRewardDef>();
        public int valueBreakpoint;
        public int lunarCoins;
        public WeightedSelection<RewardDef> selection = new WeightedSelection<RewardDef>();

        public void Add(RewardDef rewardDef)
        {
            if (rewardDef == null) 
                return;

            Add(rewardDef, 1);
        }
        public void Add(RewardDef rewardDef, float weight)
        {
            if (rewardDef == null)
                return;

            rewardDefs.Add(new WeightedRewardDef { rewardDef = rewardDef, weight = weight });
        }
        public void Regenerate(Run run)
        {
            if (run == null) 
                return;

            selection.Clear();
            for (int i = 0; i < rewardDefs.Count; i++)
            {
                RewardDef rewardDef = rewardDefs[i].rewardDef;
                if (rewardDef.isAvailable(run))
                {
                    selection.AddChoice(rewardDef, rewardDefs[i].weight);
                }
            }
        }
        public RewardDef GenerateReward(Xoroshiro128Plus rng)
        {
            if (selection.Count > 0)
            {
                return selection.Evaluate(rng.nextNormalizedFloat);
            }
            return null;
        }

    }
}
