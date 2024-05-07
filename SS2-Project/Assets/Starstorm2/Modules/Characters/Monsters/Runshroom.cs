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
    public sealed class Runshroom : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acRunshroom", SS2Bundle.Monsters);
        public override void Initialize()
        {
            if(SS2Main.ChristmasTime)
            {
                ChristmasTime();
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * GameObject - "RunshroomBody" - Monsters
             * GameObject - "RunshroomMaster" - Monsters
             * MonsterCardProvider - ??? - Monsters
             */
            yield break;
        }

        private void ChristmasTime()
        {
            CharacterPrefab.AddComponent<SantaHatPickup>();
            CharacterPrefab.AddComponent<EntityLocator>().entity = CharacterPrefab;
            CharacterPrefab.AddComponent<Highlight>().targetRenderer = CharacterPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().mainSkinnedMeshRenderer;
        }
    }
}
