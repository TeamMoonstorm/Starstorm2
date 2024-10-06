using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;

namespace SS2.Items
{
    public class PrimalBirthright : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acPrimalBirthright", SS2Bundle.Indev);

        public static float reducedGravity = 0.50f;
        public static float stackingEffect = 0.05f;

        public override void Initialize()
        {
            Stage.onStageStartGlobal += OnStageStartGlobal;
        }

        private static void OnStageStartGlobal(Stage stage)
        {
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

            DirectorCore instance = DirectorCore.instance;
            SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscFreeChest");
            DirectorPlacementRule placementRule = new DirectorPlacementRule();
            placementRule.placementMode = DirectorPlacementRule.PlacementMode.Random;
            Xoroshiro128Plus rng = new Xoroshiro128Plus(0UL);
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, rng);
            instance.TrySpawnObject(directorSpawnRequest);
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
