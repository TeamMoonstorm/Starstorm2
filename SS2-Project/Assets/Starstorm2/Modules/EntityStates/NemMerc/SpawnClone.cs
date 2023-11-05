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
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            return itemIndex != RoR2Content.Items.ExtraLife.itemIndex && itemIndex != DLC1Content.Items.ExtraLifeVoid.itemIndex;
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
                inventoryToCopy = copyInventory ? base.characterBody.inventory : null,
                inventoryItemCopyFilter = ItemFilter,
                summonerBodyObject = base.gameObject,
                loadout = loadout,
                preSpawnSetupCallback = (master) =>
                {
                    master.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = cloneLifetime;
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


                        //COPY SKILL STOCK/COOLDOWNS
                        // (DOESNT FUCKING WORK ?????????????????????????????????????????????????????))))))))))))))))))))))))
                        body.skillLocator.primary.stock = base.skillLocator.primary.stock;
                        body.skillLocator.primary.rechargeStopwatch = base.skillLocator.primary.rechargeStopwatch;
                        body.skillLocator.secondary.stock = base.skillLocator.secondary.stock;
                        body.skillLocator.secondary.rechargeStopwatch = base.skillLocator.secondary.rechargeStopwatch;
                        body.skillLocator.utility.stock = base.skillLocator.utility.stock;
                        body.skillLocator.utility.rechargeStopwatch = base.skillLocator.utility.rechargeStopwatch;
                        body.skillLocator.special.stock = base.skillLocator.special.stock;
                        body.skillLocator.special.rechargeStopwatch = base.skillLocator.special.rechargeStopwatch;
                    };
                }
            }.Perform().GetComponent<CloneInputBank>();

            clone.ownerMasterObject = this.characterBody.master.gameObject;
            clone.maxRecasts = this.numRecasts;

            

        }

       

    }
}
