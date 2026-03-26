using RoR2;
using RoR2.ContentManagement;
using R2API;
using System;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Interactables
{
    public sealed class Pedestal : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acPedestal", SS2Bundle.Interactables);
        public override void Initialize()
        {
            IL.RoR2.UI.PingIndicator.RebuildPing += PingIndicator_RebuildPing;
            SceneDirector.onGenerateInteractableCardSelection += OnGenerateInteractableCardSelection;
        }
        private static void OnGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
        {
            if (RoR2.Artifacts.CommandArtifactManager.IsCommandArtifactEnabled)
            {
                dccs.RemoveCardsThatFailFilter(new Predicate<DirectorCard>(IsPedestal));
            }
        }
        private static bool IsPedestal(DirectorCard card)
        {
            return card.GetSpawnCard().prefab.GetComponent<PedestalBehavior>();
        }

    private void PingIndicator_RebuildPing(MonoMod.Cil.ILContext il)
        {
            // TODO: check for PedestalBehavior component and add the pickup's name to the string
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta;
        }

    }
}
