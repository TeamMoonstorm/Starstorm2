using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
    public class ArrowPickup : MonoBehaviour
    {
        [Tooltip("The base object to destroy when this pickup is consumed.")]
        public GameObject baseObject;
        [Tooltip("The team filter of who can pick up this pack.")]
        public TeamFilter teamFilter;
        [Tooltip("The character body who can pick up this pack.")]
        public CharacterBody characterBody;

        //public GameObject pickupEffectPrefab;
        private bool alive = true;

        private void OnTriggerStay(Collider other)
        {
            Debug.Log("ontriggerstay");
            if (NetworkServer.active && alive)
            {
                Debug.Log("active and alive");
                if (other.gameObject.GetComponent<CharacterBody>() != null)
                {
                    Debug.Log("char body");
                    CharacterBody objectBody = other.gameObject.GetComponent<CharacterBody>();
                    if (objectBody != null)
                    {
                        Debug.Log("object body");
                        TeamIndex objectTeam = TeamComponent.GetObjectTeam(other.gameObject);
                        SkillLocator skillLocator = objectBody.skillLocator;
                        if (skillLocator != null && skillLocator.primary.stock < 7)
                        {
                            if (objectTeam == teamFilter.teamIndex && objectBody.bodyIndex == characterBody.bodyIndex)
                            {
                                alive = false;
                                Vector3 position = transform.position;
                                characterBody.skillLocator.primary.stock++;
                                Debug.Log("picked up");
                                if (characterBody.skillLocator.primary.stock > 7)
                                    characterBody.skillLocator.primary.stock = 7;
                                /*if (pickupEffectPrefab != null)
                                {
                                    EffectManager.SimpleEffect(pickupEffectPrefab, position, Quaternion.identity, true);
                                }*/
                                Destroy(baseObject);
                            }
                        }
                    }
                }
            }
        }
    }
}
