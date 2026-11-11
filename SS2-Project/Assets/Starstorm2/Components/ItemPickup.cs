using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2.Items;
using System;
using System.Collections.Generic;
namespace SS2.Components
{
    public class ItemPickup : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if (NetworkServer.active && TeamComponent.GetObjectTeam(other.gameObject) == this.teamFilter.teamIndex)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body)
                {
                    body.OnPickup(CharacterBody.PickupClass.Item);

                    if (body.inventory)
                    {
                        if (itemDecayValue != -1f)
                        {
                            body.inventory.tempItemsStorage.GiveItemTemp(itemToGrant.itemIndex, decayValue: itemDecayValue);
                        }
                        else
                        {
                            body.inventory.GiveItemPermanent(itemToGrant);
                        }
                        
                    }

                    if (pickupEffect)
                    {
                        EffectManager.SimpleEffect(pickupEffect, transform.position, Quaternion.identity, true);
                    }

                    Destroy(baseObject);
                }
            }
        }

        public ItemDef itemToGrant;
        public float itemDecayValue = -1f;

        [Tooltip("The base object to destroy when this pickup is consumed.")]
        public GameObject baseObject;

        [Tooltip("The team filter object which determines who can pick up this pack.")]
        public TeamFilter teamFilter;

        public GameObject pickupEffect;

    }
}
