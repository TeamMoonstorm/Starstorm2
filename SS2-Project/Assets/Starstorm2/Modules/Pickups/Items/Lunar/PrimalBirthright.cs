using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Items
{
    public class PrimalBirthright : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acPrimalBirthright", SS2Bundle.Indev);
        public static SpawnCard GoldChestSpawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/GoldChest/iscGoldChest.asset").WaitForCompletion();

        public static int numChestsSpawned = 0;

        public override void Initialize()
        {
            Stage.onStageStartGlobal += OnStageStartGlobal;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }

        private static void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            Debug.Log("WHAT IS THE NAME OF THE GOLD CHEST: " + self.name);

            if (PrimalBirthright.numChestsSpawned > 0)
            {
                PrimalBirthright.numChestsSpawned--;
            }

            orig(self, activator);
        }

        private static void OnStageStartGlobal(Stage stage)
        {
            PrimalBirthright.numChestsSpawned = 0;

            int birthrightCount = 0;
            foreach (CharacterMaster readOnlyInstances in CharacterMaster.readOnlyInstancesList)
            {
                if (readOnlyInstances.inventory.GetItemCount(SS2Content.Items.PrimalBirthright) > 0)
                {
                    birthrightCount++;
                }
            }

            if (birthrightCount == 0)
            {
                return;
            }

            if (!NetworkServer.active)
            {
                return;
            }
             
            for (int i = 0; i < birthrightCount; i++)
            {
                DirectorCore instance = DirectorCore.instance;
                SpawnCard spawnCard = GoldChestSpawnCard;
                DirectorPlacementRule placementRule = new DirectorPlacementRule();
                placementRule.placementMode = DirectorPlacementRule.PlacementMode.Random;
                Xoroshiro128Plus rng = new Xoroshiro128Plus(0UL);
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, rng);
                GameObject gameObj = instance.TrySpawnObject(directorSpawnRequest);

                if (gameObj != null)
                {
                    PrimalBirthright.numChestsSpawned += 1;
                }
            }
            
        }

        public class PrimalBirthrightBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef() => SS2Content.Items.PrimalBirthright;

        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
