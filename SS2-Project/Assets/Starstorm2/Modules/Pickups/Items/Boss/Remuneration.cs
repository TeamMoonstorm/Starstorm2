using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
namespace SS2.Items
{
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 
    // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION // ITS NOT RENUMERATION 

    //nice spam bro -N

    public sealed class Remuneration : SS2Item
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
        public static GameObject remunerationControllerPrefab;

        // LAZY SHITCODE ALL TIME EVER. SRY. SHOULD BE TEMPORARY. NEMESIS MERCENARY MUST RELEASE
        // ALL THE BEHAVIOR IS SPLIT UP IN LIKE 10 DIFFERENT CLASSES. HAHA LOL!. IT WILL EVENTUALLY MAKE SENSE WHEN ITS NOT JUST RED ITEMS. UNLESS I DIE BEFORE THEN.
        //i feel sorry for Orb... -N
        public override void Initialize()
        {
            On.RoR2.PickupDisplay.RebuildModel += EnableVoidParticles;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return contentPack.survivorDefs.Find("survivorNemMerc");
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<GameObject>("RemunerationController", SS2Bundle.Items);
            helper.AddAssetToLoad<ItemDef>("Remuneration", SS2Bundle.Items);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _itemDef = helper.GetLoadedAsset<ItemDef>("Remuneration");
            remunerationControllerPrefab = helper.GetLoadedAsset<GameObject>("RemunerationController");
        }

        // this works for all sibylline items but i dont know where to put general hooks like that
        // also MSU is apparently supposed to be able to do this. can remove this whenever thats fixed

        //That was orb, someone please ask him what he meant by this, he never told me what he was trying to do :,) -N
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
                base.Awake();
                Stage.onServerStageBegin += TrySpawnShop;
            }

            private void OnDestroy()
            {
                Stage.onServerStageBegin -= TrySpawnShop;
            }

            private void TrySpawnShop(Stage stage)
            {
                if (stage.sceneDef && stage.sceneDef.sceneType == SceneType.Stage)
                    base.GetComponent<CharacterMaster>().onBodyStart += SpawnPortalOnBody;
            }

            // should only happen the first time a master spawns each stage
            private void SpawnPortalOnBody(CharacterBody body)
            {
                GameObject controller = GameObject.Instantiate(remunerationControllerPrefab, body.coreTransform.position, body.coreTransform.rotation);
                controller.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);

                body.master.onBodyStart -= SpawnPortalOnBody;

            }
        }
    }
}
