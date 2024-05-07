using MSU;
using R2API;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
namespace SS2.Monsters
{
    public sealed class Lamp : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acLamp", SS2Bundle.Monsters);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }
}
