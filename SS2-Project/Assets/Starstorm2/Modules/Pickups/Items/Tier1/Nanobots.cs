using R2API;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using System;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using EntityStates;
using EntityStates.Scrapper;
namespace SS2.Items
{
    public sealed class Nanobots : SS2Item
    {
        private const string token = "SS2_ITEM_NANOBOTS_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acNanobots", SS2Bundle.Items);
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Armor per stack.")]
        [FormatToken(token, 0)]
        public static float armor = 5f;
        public override void Initialize()
        {
            //RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            //On.EntityStates.Scrapper.WaitToBeginScrapping.FixedUpdate += CheckNanobots;
            //On.RoR2.UI.ScrapperInfoPanelHelper.ShowInfo += ReplaceScrapIcon;
            //On.RoR2.ScrapperController.BeginScrapping_int += ReturnNanobots;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false; //SS2Config.enableBeta && base.IsAvailable(contentPack);
        }
        // give back original nanobots and make it so only one orb spawns
    //    private void ReturnNanobots(On.RoR2.ScrapperController.orig_BeginScrapping_int orig, ScrapperController self, int intPickupIndex)
    //    {
    //        bool isNanobots = false;
    //        PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(intPickupIndex));
    //        if (pickupDef != null && pickupDef.itemIndex == SS2Content.Items.Nanobots.itemIndex)
    //            isNanobots = true;
    //        int originalMax = self.maxItemsToScrapAtATime;
    //        int nanobotCount = 0;
    //        if(self.interactor.TryGetComponent(out CharacterBody body) && body.inventory && isNanobots)
    //        {
    //            self.maxItemsToScrapAtATime = 1;
    //            nanobotCount = Mathf.Min(self.maxItemsToScrapAtATime, body.inventory.GetItemCount(SS2Content.Items.Nanobots.itemIndex));
    //        }
    //        orig(self, intPickupIndex);
            
    //        if(body && body.inventory && isNanobots)
    //        {
    //            body.inventory.GiveItem(SS2Content.Items.Nanobots, nanobotCount);
    //            // TODO: Below broke post-AC
    //            self.itemsEaten -= nanobotCount;
    //        }
    //        self.maxItemsToScrapAtATime = originalMax;
    //    }
    //    // lazy but fine
    //    private void CheckNanobots(On.EntityStates.Scrapper.WaitToBeginScrapping.orig_FixedUpdate orig, WaitToBeginScrapping self)
    //    {
    //        orig(self);
    //        // TODO: Below broke post-AC
    //        if (self.scrapperController && self.scrapperController.lastScrappedItemIndex == SS2Content.Items.Nanobots.itemIndex && self.fixedAge > WaitToBeginScrapping.duration)
    //        {
    //            self.outer.SetNextState(new Disassemble());
    //        }
    //    }
    //    // replace scrap icon in info panel with nanobots
    //    private void ReplaceScrapIcon(On.RoR2.UI.ScrapperInfoPanelHelper.orig_ShowInfo orig, RoR2.UI.ScrapperInfoPanelHelper self, RoR2.UI.MPButton button, PickupDef pickupDef)
    //    {
    //        orig(self, button, pickupDef);
    //        if (pickupDef.itemIndex == SS2Content.Items.Nanobots.itemIndex)
    //        {
    //            self.correspondingScrapImage.sprite = pickupDef.iconSprite;
    //        }
    //    }

    //    private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    //    {
    //        int stack = sender.inventory ? sender.inventory.GetItemCount(ItemDef) : 0;
    //        args.armorAdd += armor * stack;
    //    }        
    //    public class Disassemble : ScrapperBaseState
    //    {
    //        public static float duration = 2f;
    //        public static float maxPrintHeight = 0.5f;
    //        public static float minPrintHeight = -1.8f;
    //        private MaterialPropertyBlock propBlock;
    //        private Renderer mesh;
    //        public override bool enableInteraction => false;
    //        public override void OnEnter()
    //        {
    //            // NEED SOUND EFFECT
    //            base.OnEnter();
    //            propBlock = new MaterialPropertyBlock();
    //            Material print = SS2Assets.LoadAsset<Material>("matTrimsheetScrapperPrint", SS2Bundle.Items);
    //            if(base.modelLocator.modelTransform.Find("ScrapperMesh").TryGetComponent(out SkinnedMeshRenderer renderer))
    //            {
    //                mesh = renderer;
    //                mesh.material = print;
    //            }                             
    //        }
    //        public override void FixedUpdate()
    //        {
    //            base.FixedUpdate();
    //            float t = base.fixedAge / duration;
    //            float height = Mathf.Lerp(maxPrintHeight, minPrintHeight, t);
    //            if (mesh)
    //            {
    //                mesh.GetPropertyBlock(this.propBlock);
    //                this.propBlock.SetFloat(PrintController.sliceHeightShaderPropertyId, height);
    //                mesh.SetPropertyBlock(this.propBlock);

    //            }
    //            if (base.fixedAge >= duration)
    //            {
    //                this.outer.SetNextState(new EntityStates.Scrapper.Idle());
    //            }
    //        }

    //        public override void OnExit()
    //        {
    //            base.OnExit();

    //            PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(SS2Content.Items.Nanobots.itemIndex);
    //            if (pickupIndex != PickupIndex.none)
    //            {
    //                Transform transform = base.FindModelChild(ScrappingToIdle.muzzleString);
    //                PickupDropletController.CreatePickupDroplet(pickupIndex, transform.position, Vector3.up * ScrappingToIdle.dropUpVelocityStrength + transform.forward * ScrappingToIdle.dropForwardVelocityStrength);
    //                // TODO: Below broke post-AC
    //                this.scrapperController.itemsEaten--;
    //            }

    //            Destroy(base.modelLocator.modelTransform.gameObject);
    //            Destroy(base.gameObject);
    //        }
    //    }
    //}
}