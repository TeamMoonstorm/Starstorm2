using MSU;
using R2API;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
namespace SS2.Monsters
{
    public sealed class Mimic : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acMimic", SS2Bundle.Indev);

        public static GameObject _masterPrefab;

        public override void Initialize()
        {
            _masterPrefab = AssetCollection.FindAsset<GameObject>("MimicMaster");
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }
}
