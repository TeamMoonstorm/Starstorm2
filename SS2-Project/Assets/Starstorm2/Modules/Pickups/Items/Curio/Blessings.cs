using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine.Networking;
using RoR2.Orbs;
namespace SS2.Items
{
    public sealed class Blessings : SS2Item
    {
        // scrap from chests
        // potential from chest
        // elites drop items
        // champions drop items

        // hardened shell
        // bleedout
        // shield gating/shield on kill
        // max hp on kill

        // stack crit on shrine use. convert crit chance to crit dmg
        // dmg to elites ?
        // debuffs deal % health dmg
        // more money from mobs. dmg from money?


        //storm:
        //scrap
        //shieldgate
        //elite dmg

        //fire:
        //champions
        //bleedout
        //money

        //ice:
        //elites
        //shell
        //debuffs

        //lightning:
        //potentials
        //hp
        //crit

        //earth:
        //champion
        //shell
        //elite dmg

        //poison:
        //scrap
        //hp
        //debuffs

        //gold:
        //scrap
        //shell
        //money

        //void:
        //potential
        //shield
        //crit
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ExtendedAssetCollection>("acBlessings", SS2Bundle.Items);
        private static GameObject optionPrefab;
        private static GameObject hpOrbEffect;
        private static GameObject shieldOrbEffect;
        private static GameObject critOrbEffect;
        private static BasicPickupDropTable eliteDropTable;
        private static Xoroshiro128Plus eliteDropRng;
        private static BossDropTable bossDropTable;
        private static Xoroshiro128Plus bossDropRng;
        public override void Initialize()
        {
            optionPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();
            hpOrbEffect = SS2Assets.LoadAsset<GameObject>("HpOnKillOrbEffect", SS2Bundle.Items);
            shieldOrbEffect = SS2Assets.LoadAsset<GameObject>("ShieldOnKillOrbEffect", SS2Bundle.Items);
            shieldOrbEffect = SS2Assets.LoadAsset<GameObject>("CritOnShrineOrbEffect", SS2Bundle.Items);
            eliteDropTable = SS2Assets.LoadAsset<BasicPickupDropTable>("dtItemOnEliteKill", SS2Bundle.Items);
            bossDropTable = SS2Assets.LoadAsset<BossDropTable>("dtItemOnBossKill", SS2Bundle.Items);
            On.RoR2.CharacterMaster.GiveMoney += GiveMoney; // goldengun
            On.RoR2.HealthComponent.TakeDamageProcess += TakeDamageProcess; // timing of this one might be wrong
            On.RoR2.PurchaseInteraction.OnInteractionBegin += OnInteractionBegin; // scrap, snakeeyes
            On.RoR2.ChestBehavior.Roll += ChestRoll; // option
            IL.RoR2.ChestBehavior.BaseItemDrop += ChestDrop; // option
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients; //hp, shieldgate, snakeeyes, goldengun
            Run.onRunStartGlobal += (run) =>
            {
                eliteDropRng = new Xoroshiro128Plus(run.treasureRng.nextUlong);
                bossDropRng = new Xoroshiro128Plus(run.treasureRng.nextUlong);
            };
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal; //boss, elite, shield, hp
        }

        // 10% increased money per minute
        private void GiveMoney(On.RoR2.CharacterMaster.orig_GiveMoney orig, CharacterMaster self, uint amount)
        {
            int gun = self.inventory ? self.inventory.GetItemCount(SS2Content.Items.GoldenGun) : 0;
            if (gun > 0)
            {
                int minutes = Mathf.FloorToInt(Run.FixedTimeStamp.tNow / 60f);
                float multiplier = Mathf.Pow(1 + (0.1f * gun), minutes);
                orig(self, (uint)(amount * multiplier));
                return;
            }
            orig(self, amount);
        }

        // bonus damage to elites WRONG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ga sl eak
        private void TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self.body.isElite && damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                int elite = body.inventory.GetItemCount(SS2Content.Items.BonusDamageToElites);
                if(elite > 0)
                {
                    //damageInfo.
                }
            }
            orig(self, damageInfo);
        }

        public override bool IsAvailable(ContentPack contentPack) => true;

        //storing the stacks as items because im too lazy to implement propersave
        //golden gun dmg
        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.inventory) return;
            args.baseHealthAdd += sender.inventory.GetItemCount(SS2Content.Items.StackMaxHpOnKill);
            args.baseShieldAdd += sender.inventory.GetItemCount(SS2Content.Items.StackShieldGate);
            args.critAdd += sender.inventory.GetItemCount(SS2Content.Items.StackSnakeEyes);
            if (sender.crit > 100)
            {
                // it will take a second recalcstats to calculate this correctly
                // you could do sender.crit + itemCount to fix the first recalcstats, but it would mean the crit dmg bonus is doubled afterwards
                // i stupid
                args.critDamageMultAdd += sender.crit - 100;
            }
            if (sender.master && sender.inventory.GetItemCount(SS2Content.Items.GoldenGun) > 0)
            {
                int maxStacks = 20; /////////////////////////////////////////////////////////////////////////////////////////////
                float smallChest = Mathf.Min(sender.master.money / Run.instance.GetDifficultyScaledCost(25), maxStacks);
                args.damageMultAdd += .05f * smallChest;
            }

        }

        // guaranteed items on boss kill. chance for boss item
        // chance for items on elite kill
        // hp and shield on kill
        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            Inventory inventory = damageReport.attackerMaster ? damageReport.attackerMaster.inventory : null;
            if(inventory)
            {
                int hp = inventory.GetItemCount(SS2Content.Items.MaxHpOnKill);
                int shield = inventory.GetItemCount(SS2Content.Items.ShieldGate);
                Vector3 origin = damageReport.victimBody ? damageReport.victimBody.corePosition : Vector3.zero;
                if(hp > 0)
                {
                    GrantItemOrb orb = new GrantItemOrb();
                    orb.origin = origin;
                    orb.target = Util.FindBodyMainHurtBox(damageReport.attackerBody);
                    orb.item = SS2Content.Items.StackMaxHpOnKill.itemIndex;
                    orb.count = 1 + hp;
                    orb.effectPrefab = hpOrbEffect;
                    OrbManager.instance.AddOrb(orb);
                }
                if (shield > 0)
                {
                    GrantItemOrb orb = new GrantItemOrb();
                    orb.origin = origin;
                    orb.target = Util.FindBodyMainHurtBox(damageReport.attackerBody);
                    orb.item = SS2Content.Items.StackShieldGate.itemIndex;
                    orb.count = 1 + shield;
                    orb.effectPrefab = shieldOrbEffect;
                    OrbManager.instance.AddOrb(orb);
                }
            }
            if (damageReport.victimTeamIndex == TeamIndex.Player || !damageReport.victimBody) return;
            int eliteItems = SS2Util.GetItemCountForPlayers(SS2Content.Items.ItemOnEliteKill);
            int bossItems = SS2Util.GetItemCountForPlayers(SS2Content.Items.ItemOnBossKill);
            if (eliteItems > 0 && damageReport.victimIsElite && Util.CheckRoll(4f + 1f * (eliteItems - 1))) // realizing i have no idea how to use run seeds/rng correctly
            {
                PickupIndex pickupIndex = eliteDropTable.GenerateDrop(eliteDropRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
            if (bossItems > 0 && damageReport.victimIsChampion)
            {
                PickupIndex pickupIndex = PickupIndex.none;
                if (Util.CheckRoll(1 - Mathf.Pow(0.95f, bossItems)) && damageReport.victimBody.TryGetComponent(out DeathRewards deathRewards))
                {
                    pickupIndex = deathRewards.bossDropTable ? deathRewards.bossDropTable.GenerateDrop(bossDropRng) : (PickupIndex)deathRewards.bossPickup;
                }
                if (pickupIndex == PickupIndex.none)
                    pickupIndex = bossDropTable.GenerateDrop(bossDropRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
        }

        //lower the tier 1 weight of chests, increasing "rarity"
        private void ChestRoll(On.RoR2.ChestBehavior.orig_Roll orig, ChestBehavior self)
        {
            int option = SS2Util.GetItemCountForPlayers(SS2Content.Items.OptionFromChest);
            float tier1Coefficient = Mathf.Pow(0.85f, option);
            BasicPickupDropTable dropTable = null;
            bool valid = self && self.dropTable && (dropTable = (BasicPickupDropTable)self.dropTable) != null;
            if(!valid)
            {
                orig(self);
                return;            
            }
            dropTable.tier1Weight *= tier1Coefficient; // lol.
            dropTable.GenerateWeightedSelection(Run.instance);
            orig(self);
            dropTable.tier1Weight /= tier1Coefficient;
            dropTable.GenerateWeightedSelection(Run.instance);
        }

        //chance to turn each chest drop into a void potential
        private void ChestDrop(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // GenericPickupController.CreatePickupInfo createPickupInfo = default(GenericPickupController.CreatePickupInfo);
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloca(3),
                x => x.MatchInitobj<GenericPickupController.CreatePickupInfo>());
            if(b)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<GenericPickupController.CreatePickupInfo, ChestBehavior>>((info, chest) =>
                {
                    int option = SS2Util.GetItemCountForPlayers(SS2Content.Items.OptionFromChest);
                    float chance = 1f - Mathf.Pow(0.7f, option);
                    if(Util.CheckRoll(chance))
                    {
                        info.prefabOverride = optionPrefab;
                        info.pickerOptions = PickupPickerController.GenerateOptionsFromArray(chest.dropTable.GenerateUniqueDrops(3, chest.rng));
                        info.pickupIndex = PickupCatalog.FindPickupIndex(PickupCatalog.GetPickupDef(chest.dropPickup).itemTier); // idk if we need to do this? void potentials use the pickupdef for the tier
                    }
                });
            }
        }

        // chance to add 1 to chest drops, then replace the first drop with scrap
        // grant crit chance on shrine
        private void OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            if(self.isShrine && activator && activator.TryGetComponent(out CharacterBody body) && body.inventory)
            {
                int crit = body.inventory.GetItemCount(SS2Content.Items.SnakeEyes);
                if(crit > 0)
                {
                    GrantItemOrb orb = new GrantItemOrb();
                    orb.origin = self.transform.position;
                    orb.target = Util.FindBodyMainHurtBox(body);
                    orb.item = SS2Content.Items.StackSnakeEyes.itemIndex;
                    orb.count = 3 + 2 * crit;
                    orb.effectPrefab = critOrbEffect;
                    OrbManager.instance.AddOrb(orb);
                }               
            }
            int scrap = SS2Util.GetItemCountForPlayers(SS2Content.Items.ScrapFromChest);         
            if (self.TryGetComponent(out ChestBehavior chest))
            {             
                if(scrap > 0 && Util.CheckRoll(20f + 5 * (scrap - 1)))
                {
                    PickupIndex pickup = chest.dropTable.GenerateDrop(chest.rng);
                    if (pickup != PickupIndex.none)
                    {
                        PickupIndex scrapIndex = PickupCatalog.FindScrapIndexForItemTier(PickupCatalog.GetPickupDef(pickup).itemTier);
                        if (scrapIndex != PickupIndex.none)
                        {
                            chest.dropPickup = scrapIndex;
                            chest.dropCount++;
                        }
                    }
                }
                             
            }
        }

        
    }
    public class GrantItemOrb : RoR2.Orbs.Orb
    {
        public override void Begin()
        {
            base.duration = base.distanceToTarget / 30f;
            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };
            effectData.SetHurtBoxReference(this.target);
            EffectManager.SpawnEffect(effectPrefab, effectData, true);
            HurtBox component = this.target.GetComponent<HurtBox>();
            CharacterBody characterBody = (component != null) ? component.healthComponent.GetComponent<CharacterBody>() : null;
            if (characterBody)
            {
                this.targetInventory = characterBody.inventory;
            }
        }
        public override void OnArrival()
        {
            if (this.targetInventory)
            {
                this.targetInventory.GiveItem(item, count);
            }
        }

        public float speed = 30f;

        public int count;
        public ItemIndex item;
        public GameObject effectPrefab;
        private Inventory targetInventory;
    }
}
