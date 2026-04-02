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
            SceneDirector.onGenerateInteractableCardSelection += OnGenerateInteractableCardSelection;
        }

        private static void OnGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
        {
            if (Run.instance.selectedDifficulty != Typhoon.sdd.DifficultyIndex)
            {
                dccs.RemoveCardsThatFailFilter(new Predicate<DirectorCard>(IsNotAmbush));
            }
        }
        private static bool IsNotAmbush(DirectorCard card)
        {
            return !card.GetSpawnCard().prefab.GetComponent<AmbushBehavior>();
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta;
        }

    }
}
