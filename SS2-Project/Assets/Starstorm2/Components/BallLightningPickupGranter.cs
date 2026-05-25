using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2.Items;

namespace SS2.Components
{
    public class BallLightningPickupGranter : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if (NetworkServer.active && alive && TeamComponent.GetObjectTeam(other.gameObject) == teamFilter.teamIndex)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body)
                {
                    body.OnPickup(CharacterBody.PickupClass.Item);

                    if (body.inventory)
                    {
                        body.inventory.SetEquipmentIndex(SS2Content.Equipments.BallLightning.equipmentIndex);
                    }
                    if (pickupEffect)
                    {
                        EffectManager.SpawnEffect(pickupEffect, new EffectData
                        {
                            origin = transform.position
                        }, true);
                    }

                    GameObject.Destroy(gameObject);
                }
            }
        }

        [Tooltip("The base object to destroy when this pickup is consumed.")]
        public GameObject baseObject;

        [Tooltip("The team filter object which determines who can pick up this pack.")]
        public TeamFilter teamFilter;

        public GameObject pickupEffect;

        private bool alive = true;
    }
}
