using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;
using RoR2.Navigation;
using Moonstorm.Starstorm2;

namespace EntityStates.NemMerc
{
    public class SpawnClone : BaseSkillState
    {
        public static float cloneLifetime = 12f;
        public static float targetedHologramOffset = 8f;
        public static GameObject hologramPrefab;
        public static GameObject scanEffect;
        public static float maxRange = 50f;
        public static bool copyInventory;

        [SerializeField]
        public int numRecasts = 1;

        private NemMercTracker tracker;


        private uint primary;
        private uint secondary;
        private uint utility;
        private uint special;

        private static List<ItemDef> illegalItems = new List<ItemDef>
            {
                RoR2Content.Items.ExtraLife,
                DLC1Content.Items.ExtraLifeVoid,
                RoR2Content.Items.RoboBallBuddy,
                RoR2Content.Items.BeetleGland,
                RoR2Content.Items.FocusConvergence,
                RoR2Content.Items.TPHealingNova,
                RoR2Content.Items.Squid,
                RoR2Content.Items.TitanGoldDuringTP,
                DLC1Content.Items.DroneWeapons,
                DLC1Content.Items.FreeChest,
                DLC1Content.Items.VoidMegaCrabItem,
                //DLC1Content.Items.MinorConstructOnKill,
                RoR2Content.Items.TreasureCache,
                DLC1Content.Items.TreasureCacheVoid,
                SS2Content.Items.RelicOfTermination,
                SS2Content.Items.NkotasHeritage,
                SS2Content.Items.Remuneration,
            };

        public override void OnEnter()
        {
            base.OnEnter();

            this.tracker = base.GetComponent<NemMercTracker>();

            primary = base.characterBody.master.loadout.bodyLoadoutManager.GetSkillVariant(base.characterBody.bodyIndex, 0);
            secondary = base.characterBody.master.loadout.bodyLoadoutManager.GetSkillVariant(base.characterBody.bodyIndex, 1);
            utility = base.characterBody.master.loadout.bodyLoadoutManager.GetSkillVariant(base.characterBody.bodyIndex, 2);
            special = base.characterBody.master.loadout.bodyLoadoutManager.GetSkillVariant(base.characterBody.bodyIndex, 3);

            this.SpawnHolograms();

            //anim

            Util.PlaySound("Play_nemmerc_clone_spawn", base.gameObject);
            EffectManager.SpawnEffect(scanEffect, new EffectData
            {
                origin = base.transform.position,
                scale = 60f,
            }, false);
            this.outer.SetNextStateToMain();
        }

        private void SpawnHolograms()
        {
            if (!NetworkServer.active) return;

            if(this.tracker)
            {
                GameObject target = this.tracker.GetTrackingTarget();
                if(target)
                {
                    Vector3 direction = (target.transform.position - base.transform.position).normalized;

                    Vector3 position = target.transform.position + (direction * SpawnClone.targetedHologramOffset);
                    if (Physics.Raycast(target.transform.position, direction, out RaycastHit hit, SpawnClone.targetedHologramOffset, LayerIndex.world.mask))
                    {
                        position = hit.point;
                    }
                    this.SpawnHologramSingle(position);
                    return;
                }            
            }

            Vector3 point = base.inputBank.aimDirection * maxRange + base.inputBank.aimOrigin;
            if(Physics.Raycast(base.inputBank.aimOrigin, base.inputBank.aimDirection, out RaycastHit hit2, SpawnClone.maxRange, LayerIndex.world.mask))
            {
                point = hit2.point;
            }
                    
            this.SpawnHologramSingle(point);

        }

        public static bool ItemFilter(ItemIndex itemIndex)
        {
            var def = ItemCatalog.GetItemDef(itemIndex);
            if(def.name == "ITEM_ANCIENT_SCEPTER" || def.nameToken == "ITEM_ANCIENT_SCEPTER_NAME")
            {
                return false;
            }
            foreach(var item in illegalItems)
            {
                if(item.itemIndex == itemIndex)
                {
                    return false;
                }
            }
            return true;
        }

        private void SpawnHologramSingle(Vector3 position)
        {
            // LOADOUTS MAKE NO FUCKING SENSE           

            CharacterMaster holo = hologramPrefab.GetComponent<CharacterMaster>();
            Loadout loadout = new Loadout();
            holo.loadout.Copy(loadout);
            BodyIndex index = holo.bodyPrefab.GetComponent<CharacterBody>().bodyIndex;

            loadout.bodyLoadoutManager.SetSkillVariant(index, 0, primary);
            loadout.bodyLoadoutManager.SetSkillVariant(index, 1, secondary);
            loadout.bodyLoadoutManager.SetSkillVariant(index, 2, utility);
            loadout.bodyLoadoutManager.SetSkillVariant(index, 3, special);

            CloneInputBank clone = new MasterSummon
            {
                masterPrefab = hologramPrefab,
                position = position,
                rotation = base.characterBody.transform.rotation,
                inventoryToCopy = null,//copyInventory ? base.characterBody.inventory : null,
                //inventoryItemCopyFilter = ItemFilter,
                summonerBodyObject = base.gameObject,
                loadout = loadout,
                preSpawnSetupCallback = (master) =>
                {
                    master.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = cloneLifetime;

                    // HAVE TO DO THIS MANUALLY. MasterSummon.inventoryItemCopyFilter does NOTHING! HEEHAHAHAEHEAH! GOOD ONE HOPO!!!!!!!!!!
                    master.inventory.itemAcquisitionOrder.Clear();
                    int[] array = master.inventory.itemStacks;
                    int num = 0;
                    HG.ArrayUtils.SetAll<int>(array, num);
                    master.inventory.AddItemsFrom(base.characterBody.inventory, ItemFilter);
                    master.inventory.CopyEquipmentFrom(base.characterBody.inventory);
                    master.onBodyStart += (body) =>
                    {
                        body.GetComponent<NemMercCloneTracker>().ownerTracker = this.tracker;

                        ////COPY BAND COOLDOWNS
                        //// SHOULD PROBABLY JUST DISABLE BANDS LMAO
                        //float ringcd = base.characterBody.GetBuffCount(RoR2Content.Buffs.ElementalRingsCooldown);
                        //int num12 = 1; //  c v
                        //while ((float)num12 <= ringcd)
                        //{
                        //    body.AddTimedBuff(RoR2Content.Buffs.ElementalRingsCooldown, (float)num12);
                        //    num12++;
                        //}



                        var bitch = body.gameObject.AddComponent<StupidFuckingCooldownSetter>();
                        bitch.primaryStock = base.skillLocator.primary.stock;
                        bitch.primaryStopwatch = base.skillLocator.primary.rechargeStopwatch;
                        bitch.secondaryStock = base.skillLocator.secondary.stock;
                        bitch.secondaryStopwatch = base.skillLocator.secondary.rechargeStopwatch;
                        bitch.utilityStock = base.skillLocator.utility.stock;
                        bitch.utilityStopwatch = base.skillLocator.utility.rechargeStopwatch;
                        bitch.specialStock = base.skillLocator.special.stock;
                        bitch.specialStopwatch = base.skillLocator.special.rechargeStopwatch;

                    };
                }
            }.Perform().GetComponent<CloneInputBank>();

            clone.ownerMasterObject = this.characterBody.master.gameObject;
            clone.maxRecasts = this.numRecasts;

            

        }

    }
}
