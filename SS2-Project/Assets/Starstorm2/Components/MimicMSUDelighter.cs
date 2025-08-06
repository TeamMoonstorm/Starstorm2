using UnityEngine;
using RoR2;
using JetBrains.Annotations;


/// <summary>
/// Exists to make MSU happy when it expects an interactable to contain an IInteractable - this goes on the Master, which otherwise would lack it
/// </summary>
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
