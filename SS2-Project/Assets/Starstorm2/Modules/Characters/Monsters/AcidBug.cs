using RoR2;
using UnityEngine;
using System;
using SS2.Components;
using MSU;
using R2API;
using RoR2.ContentManagement;
using System.Collections;

namespace SS2.Monsters
{
    public sealed class AcidBug : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acAcidBug", SS2Bundle.Monsters);

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }
        public override void Initialize()
        {
            var larvaFamily = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/DLC1/Common/dccsAcidLarvaFamily.asset").WaitForCompletion();

            if (larvaFamily && larvaFamily is DirectorCardCategorySelection category)
            {
                var card = new DirectorCard
                {
                    spawnCard = SS2Assets.LoadAsset<SpawnCard>("cscAcidBug", SS2Bundle.Monsters),
                    selectionWeight = 1,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = false,
                    minimumStageCompletions = 0,
                    requiredUnlockableDef = null,
                    forbiddenUnlockableDef = null,
                };

                category.AddCard(0, card); // add AcidBug to the "Basic Monsters" category, with AcidLarva
            }
        }
    }
}