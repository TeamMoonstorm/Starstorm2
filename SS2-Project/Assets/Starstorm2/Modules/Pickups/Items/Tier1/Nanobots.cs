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

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }
        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            On.RoR2.UI.ScrapperInfoPanelHelper.ShowInfo += ReplaceScrapIcon;
            On.RoR2.ScrapperController.BeginScrapping_UniquePickup += ReturnNanobots;
        }

        private void ReturnNanobots(On.RoR2.ScrapperController.orig_BeginScrapping_UniquePickup orig, ScrapperController self, UniquePickup pickupToTake)
        {
            // Check if we are about to try to scrap nanobots
            bool isNanobots = false;
            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupToTake.pickupIndex);
            if (pickupDef != null && pickupDef.itemIndex == SS2Content.Items.Nanobots.itemIndex)
                isNanobots = true;

            // ensure the scrapper only takes one stack of nanobots instead of all of them
            // make scrapper get set to custom state
            int originalMax = self.maxItemsToScrapAtATime;
            var originalState = self.entityStateOnScrap;
            int nanobotCount = 0;
            if (self.interactor.TryGetComponent(out CharacterBody body) && body.inventory && isNanobots)
            {
                self.maxItemsToScrapAtATime = 1;
                nanobotCount = Mathf.Min(self.maxItemsToScrapAtATime, body.inventory.GetItemCount(SS2Content.Items.Nanobots.itemIndex));

                self.entityStateOnScrap = new SerializableEntityStateType(typeof(WaitToBeginDisassembling));
            }

            // run original method to take items from the player's inventory and set the Scrapper's state to its entityStateOnScrap
            orig(self, pickupToTake);

            // give back nanobot stacks that got taken by the scrapper
            // commented out for now. scrapper will drop 2 nanobot stacks instead of keeping one in the inventory
            //if (body && body.inventory && isNanobots)
            //{
            //    body.inventory.GiveItemPermanent(SS2Content.Items.Nanobots, nanobotCount);
            //}

            // restore original scrapper behavior
            self.maxItemsToScrapAtATime = originalMax;
            self.entityStateOnScrap = originalState;
        }

        // replace scrap icon in info panel with nanobots
        private void ReplaceScrapIcon(On.RoR2.UI.ScrapperInfoPanelHelper.orig_ShowInfo orig, RoR2.UI.ScrapperInfoPanelHelper self, RoR2.UI.MPButton button, PickupDef pickupDef)
        {
            orig(self, button, pickupDef);
            if (pickupDef.itemIndex == SS2Content.Items.Nanobots.itemIndex)
            {
                self.correspondingScrapImage.sprite = pickupDef.iconSprite;
            }
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int stack = sender.inventory ? sender.inventory.GetItemCountEffective(ItemDef) : 0;
            args.armorAdd += armor * stack;
        }

        public class WaitToBeginDisassembling : ScrapperBaseState
        {
            public static float duration = 1.5f;

            public override bool enableInteraction
            {
                get
                {
                    return _impatient;
                }
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (fixedAge > duration)
                {
                    outer.SetNextState(new Disassemble());
                }
            }
        }

        public class Disassemble : ScrapperBaseState
        {
            public static float duration = 2f;
            public static float maxPrintHeight = 0.5f;
            public static float minPrintHeight = -1.8f;
            public static float dropAngle = 30f;
            public static int maxDrops = 2;

            private MaterialPropertyBlock propBlock;
            private Renderer mesh;
            public override bool enableInteraction => false;
            public override void OnEnter()
            {
                // NEED SOUND EFFECT
                base.OnEnter();
                propBlock = new MaterialPropertyBlock();
                Material print = SS2Assets.LoadAsset<Material>("matTrimsheetScrapperPrint", SS2Bundle.Items);
                if (base.modelLocator.modelTransform.Find("ScrapperMesh").TryGetComponent(out SkinnedMeshRenderer renderer))
                {
                    mesh = renderer;
                    mesh.material = print;
                }
            }
            public override void FixedUpdate()
            {
                base.FixedUpdate();
                float t = base.fixedAge / duration;
                float height = Mathf.Lerp(maxPrintHeight, minPrintHeight, t);
                if (mesh)
                {
                    mesh.GetPropertyBlock(this.propBlock);
                    this.propBlock.SetFloat(PrintController.sliceHeightShaderPropertyId, height);
                    mesh.SetPropertyBlock(this.propBlock);

                }
                if (base.fixedAge >= duration)
                {
                    this.outer.SetNextState(new EntityStates.Scrapper.Idle());
                }
            }

            public override void OnExit()
            {
                base.OnExit();

                PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(SS2Content.Items.Nanobots.itemIndex);
                if (pickupIndex != PickupIndex.none)
                {
                    Transform transform = base.FindModelChild(ScrappingToIdle.muzzleString);

                    float angle = -dropAngle / maxDrops;
                    for (int i = 0; i < maxDrops; i++)
                    {
                        float d = ScrappingToIdle.dropForwardVelocityStrength;
                        Vector3 velocity = Quaternion.Euler(0f, angle, 0f) * transform.forward * d;
                        velocity.y = ScrappingToIdle.dropUpVelocityStrength;

                        PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
                        {
                            pickup = new UniquePickup(pickupIndex),
                            position = transform.position
                        }, transform.position, velocity);

                        angle += dropAngle / maxDrops;
                    }
                    // TODO: Below broke post-AC
                    // itemsEaton doesnt exist anymore and i dont remember what it did
                    //this.scrapperController.itemsEaten--;
                }

                Destroy(base.modelLocator.modelTransform.gameObject);
                Destroy(base.gameObject);
            }
        }
    }
}