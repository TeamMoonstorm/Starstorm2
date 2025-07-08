using UnityEngine;
using RoR2;
using JetBrains.Annotations;

namespace SS2.Components
{
    public class MimicMSUDelighter : MonoBehaviour, IInteractable
    {
        public string GetContextString([NotNull] Interactor activator)
        {
            return "You shouldn't see this";
        }

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            return Interactability.Disabled;
        }

        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            
        }

        public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator)
        {
            return true;
        }

        public bool ShouldProximityHighlight()
        {
            return true;
        }

        public bool ShouldShowOnScanner()
        {
            return false;
        }
    }
}
