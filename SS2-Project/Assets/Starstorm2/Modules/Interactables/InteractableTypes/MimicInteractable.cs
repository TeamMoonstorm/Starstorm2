using RoR2;
using UnityEngine;
using RoR2.ContentManagement;

namespace SS2.Interactables
{
    public sealed class MimicInteractable : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest =>  SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acMimicInteractable", SS2Bundle.Monsters);

        public override void Initialize()
        {

        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
