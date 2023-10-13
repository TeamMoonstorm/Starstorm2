﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;
using RoR2.Navigation;
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

        private NemMercTracker tracker;

        public override void OnEnter()
        {
            base.OnEnter();

            this.tracker = base.GetComponent<NemMercTracker>();

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

        public static bool ItemFilter(ItemIndex itemIndex) // NEEDS CONFIG 
        {
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            return itemIndex != RoR2Content.Items.ExtraLife.itemIndex && itemIndex != DLC1Content.Items.ExtraLifeVoid.itemIndex;
        }

        private void SpawnHologramSingle(Vector3 position)
        {
            // LOADOUTS MAKE NO FUCKING SENSE
            uint primary = base.characterBody.master.loadout.bodyLoadoutManager.GetSkillVariant(base.characterBody.bodyIndex, 0);
            uint secondary = base.characterBody.master.loadout.bodyLoadoutManager.GetSkillVariant(base.characterBody.bodyIndex, 1);
            uint utility = base.characterBody.master.loadout.bodyLoadoutManager.GetSkillVariant(base.characterBody.bodyIndex, 2);
            uint special = base.characterBody.master.loadout.bodyLoadoutManager.GetSkillVariant(base.characterBody.bodyIndex, 3);

            new MasterSummon
            {
                masterPrefab = hologramPrefab,
                position = position,
                rotation = base.characterBody.transform.rotation,
                inventoryToCopy = copyInventory ? base.characterBody.inventory : null,
                inventoryItemCopyFilter = ItemFilter,
                summonerBodyObject = base.gameObject,
                loadout = base.characterBody.master.loadout,  /// ?????????????????????????????????????????? DOESNT FUCKING DO ANYTHING?????????????????????             
                preSpawnSetupCallback = (master) =>
                {
                    master.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = cloneLifetime;
                    master.onBodyStart += (body) =>
                    {
                        body.GetComponent<NemMercCloneTracker>().ownerTracker = this.tracker;

                        //COPY BAND COOLDOWNS

                        float ringcd = base.characterBody.GetBuffCount(RoR2Content.Buffs.ElementalRingsCooldown);
                        int num12 = 1; //  c v
                        while ((float)num12 <= ringcd)
                        {
                            body.AddTimedBuff(RoR2Content.Buffs.ElementalRingsCooldown, (float)num12);
                            num12++;
                        }
                        

                        //COPY SKILL STOCK/COOLDOWNS
                        body.skillLocator.primary.stock = base.skillLocator.primary.stock;
                        body.skillLocator.primary.rechargeStopwatch = base.skillLocator.primary.rechargeStopwatch;
                        body.skillLocator.secondary.stock = base.skillLocator.secondary.stock;
                        body.skillLocator.secondary.rechargeStopwatch = base.skillLocator.secondary.rechargeStopwatch;
                        body.skillLocator.utility.stock = base.skillLocator.utility.stock;
                        body.skillLocator.utility.rechargeStopwatch = base.skillLocator.utility.rechargeStopwatch;
                        body.skillLocator.special.stock = base.skillLocator.special.stock;
                        body.skillLocator.special.rechargeStopwatch = base.skillLocator.special.rechargeStopwatch;
                        ///////////////////////////???????????????????????????????????????????????????????????????????????????????????
                        Loadout loadout = new Loadout();
                        body.master.loadout.Copy(loadout);

                        loadout.bodyLoadoutManager.SetSkillVariant(body.bodyIndex, 0, primary);
                        loadout.bodyLoadoutManager.SetSkillVariant(body.bodyIndex, 1, secondary);
                        loadout.bodyLoadoutManager.SetSkillVariant(body.bodyIndex, 2, utility);
                        loadout.bodyLoadoutManager.SetSkillVariant(body.bodyIndex, 3, special);

                        body.master.SetLoadoutServer(loadout);
                        body.SetLoadoutServer(loadout);
                    };
                }
            }.Perform();

        }

    }
}