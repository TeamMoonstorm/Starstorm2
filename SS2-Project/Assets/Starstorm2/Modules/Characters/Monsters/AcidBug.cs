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
        public override void Initialize()
        {
            
        }
    }
}