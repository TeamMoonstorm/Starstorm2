using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using R2API;
using EntityStates;
using MSU.Config;
using RoR2.Skills;

namespace SS2.Survivors
{
    public sealed class NemMage : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemMage", SS2Bundle.Indev);

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }

        public override void Initialize()
        {
        }
    }
}