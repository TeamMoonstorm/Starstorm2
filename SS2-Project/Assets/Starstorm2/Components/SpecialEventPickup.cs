using JetBrains.Annotations;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Components
{
    public class SpecialEventPickup : MonoBehaviour, IInteractable
    {
        public ItemDef itemDef;
        public string contextToken;
        public string GetContextString([NotNull] Interactor activator)
        {
            return contextToken;
        }

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            return (body && body.inventory && body.inventory.GetItemCount(itemDef.itemIndex) == 0) ? Interactability.Available : Interactability.Disabled;
        }

        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            if (body && body.inventory)
            {
                body.inventory.GiveItem(itemDef);
                //Destroy(base.gameObject);
                NetworkServer.Destroy(base.gameObject);
            }
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

        public static SpecialEventPickup AddAndSetupComponent(CharacterBody bodyPrefab)
        {
            var gameObject = bodyPrefab.gameObject;
            if(!Util.IsPrefab(gameObject))
            {
                return null;
            }

            var pickup = gameObject.AddComponent<SpecialEventPickup>();
            pickup.EnsureComponent<EntityLocator>().entity = gameObject;
            pickup.EnsureComponent<Highlight>().targetRenderer = gameObject.GetComponentInChildren<CharacterModel>().mainSkinnedMeshRenderer;
            return pickup;
        }
    }
}
