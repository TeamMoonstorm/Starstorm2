using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
namespace Moonstorm.Starstorm2.Components
{   
    public class SantaHatPickup : MonoBehaviour, IInteractable
    {
       
        public string GetContextString([NotNull] Interactor activator)
        {
            return Language.GetString("SS2_INTERACTABLE_SANTAHAT_CONTEXT");
        }

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            return (body && body.inventory && body.inventory.GetItemCount(SS2Content.Items.SantaHat.itemIndex) == 0) ? Interactability.Available : Interactability.Disabled;
        }

        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            CharacterBody body = activator.GetComponent<CharacterBody>();
            if (body && body.inventory)
            {
                body.inventory.GiveItem(SS2Content.Items.SantaHat);
                //Destroy(base.gameObject);
                NetworkServer.Destroy(base.gameObject);
            }
        }

        public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator)
        {
            return true;
        }

        public bool ShouldShowOnScanner()
        {
            return false;
        }
    }
}
