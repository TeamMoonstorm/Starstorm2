using MSU;
using R2API;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
namespace SS2.Monsters
{
    public sealed class Green : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acGreen", SS2Bundle.Indev);


        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

    }
}
