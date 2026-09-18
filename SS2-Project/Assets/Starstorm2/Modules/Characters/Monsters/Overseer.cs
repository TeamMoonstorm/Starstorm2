using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
namespace SS2.Monsters
{
    public sealed class Overseer : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acOverseer", SS2Bundle.Indev);

        public override void Initialize()
        {
            
        }
    }
}
