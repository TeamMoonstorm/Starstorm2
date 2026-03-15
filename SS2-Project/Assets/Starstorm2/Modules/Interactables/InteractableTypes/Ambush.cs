using RoR2;
using RoR2.ContentManagement;
using R2API;
using System;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Interactables
{
    public sealed class Ambush : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acAmbush", SS2Bundle.Interactables);
        public override void Initialize()
        {

        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta;
        }

    }
}
