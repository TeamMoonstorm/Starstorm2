using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using UnityEngine.AddressableAssets;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Moonstorm.Starstorm2.Items
{
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    public sealed class Remuneration : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Remuneration", SS2Bundle.Items);

        //[RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Chance to gain soul initially. (1 = 100%)")]
        //[TokenModifier("SS2_ITEM_REMUNERATION_DESC", StatTypes.MultiplyByN, 0, "100")]
        //public static float initChance = 0.005f;

        //[RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Soul gain chance cap. (1 = 100%)")]
        //public static float maxChance = 0.1f;


        public static GameObject remunerationControllerPrefab;

        // LAZY SHITCODE ALL TIME EVER. SRY. SHOULD BE TEMPORARY. NEMESIS MERCENARY MUST RELEASE
        // ALL THE BEHAVIOR IS SPLIT UP IN LIKE 10 DIFFERENT CLASSES. HAHA LOL!. IT WILL EVENTUALLY MAKE SENSE WHEN ITS NOT JUST RED ITEMS. UNLESS I DIE BEFORE THEN.
        public override void Initialize()
        {
            base.Initialize();
            remunerationControllerPrefab = SS2Assets.LoadAsset<GameObject>("RemunerationController", SS2Bundle.Items);

            On.RoR2.PickupDisplay.RebuildModel += EnableVoidParticles;
        }


        // this works for all sibylline items but i dont know where to put general hooks like that

        // also MSU is apparently supposed to be able to do this. can remove this whenever thats fixed
        private void EnableVoidParticles(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self)
        {
            orig(self);            
            PickupDef pickupDef = PickupCatalog.GetPickupDef(self.pickupIndex);
            ItemIndex itemIndex = (pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None;
            if (itemIndex != ItemIndex.None && ItemCatalog.GetItemDef(itemIndex).tier == SS2Content.ItemTierDefs.Sibylline.tier)
            {
                if (self.voidParticleEffect)
                {
                    self.voidParticleEffect.SetActive(true);
                }             
            }
        }

        public sealed class RemunerationBehavior : BaseItemMasterBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Remuneration;

            private void Awake()
            {
                Stage.onServerStageBegin += TrySpawnShop;
            }

            private void OnDestroy()
            {
                Stage.onServerStageBegin -= TrySpawnShop;
            }

            private void TrySpawnShop(Stage stage)
            {    
                if(stage.sceneDef && stage.sceneDef.sceneType == SceneType.Stage)
                    base.GetComponent<CharacterMaster>().onBodyStart += SpawnPortalOnBody;
            }

            // should only happen the first time a master spawns each stage
            private void SpawnPortalOnBody(CharacterBody body)
            {
                GameObject controller = GameObject.Instantiate(remunerationControllerPrefab, body.coreTransform.position, body.coreTransform.rotation);
                controller.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
                //NetworkServer.Spawn(controller);

                body.master.onBodyStart -= SpawnPortalOnBody;
                
            }
        }
    }
}
