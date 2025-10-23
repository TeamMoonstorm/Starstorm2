using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
namespace SS2.Items
{
    public sealed class Remuneration : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acRemuneration", SS2Bundle.Items);
        public static GameObject remunerationControllerPrefab;

        public static GameObject deleteEffectPrefab;
        public override void Initialize()
        {
            remunerationControllerPrefab = AssetCollection.FindAsset<GameObject>("RemunerationController");
            On.RoR2.PickupDisplay.RebuildModel += EnableVoidParticles;

            deleteEffectPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab").WaitForCompletion(), "WAWA", false);
            deleteEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_nullifier_death_vortex_explode"; // cringe.
        }

        // this works for all sibylline items but i dont know where to put general hooks like that
        // also MSU is apparently supposed to be able to do this. can remove this whenever thats fixed

        //That was orb, someone please ask him what he meant by this, he never told me what he was trying to do :,) -N
        private void EnableVoidParticles(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self, GameObject modelObjectOverride)
        {
            orig(self, modelObjectOverride);
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

        public sealed class RemunerationBehavior : BaseItemMasterBehaviour
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
