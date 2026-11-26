using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace SS2.Components
{
    public class SS2BuffPickup : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if (NetworkServer.active && TeamComponent.GetObjectTeam(other.gameObject) == this.teamFilter.teamIndex)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body)
                {
                    body.OnPickup(CharacterBody.PickupClass.Minor);

                    AddBuff(body);
                    
                    if (pickupEffect)
                    {
                        EffectManager.SimpleEffect(pickupEffect, transform.position, Quaternion.identity, true);
                    }

                    Destroy(baseObject);
                }
            }
        }

        private void AddBuff(CharacterBody body)
        {
            if (buffDef)
            {
                if (buffDuration != -1f)
                {
                    if (countdown)
                    {
                        // buffs as a countdown timer, like bands
                        int stackDuration = 1;
                        while (stackDuration <= buffDuration)
                        {
                            body.AddTimedBuff(buffDef, stackDuration);
                            stackDuration++;
                        }
                    }
                    else if (maxStacks != -1)
                    {
                        // timed buff w maximum stacks
                        body.AddTimedBuff(buffDef, buffDuration, maxStacks);
                    }
                    else
                    {
                        // timed buff
                        body.AddTimedBuff(buffDef, buffDuration);
                    }
                }
                else if (body.GetBuffCount(buffDef) < maxStacks || maxStacks == -1)
                {
                    // permanent buff, with or without maximum stacks
                    body.AddBuff(buffDef);
                }
            }
        }

        public BuffDef buffDef;
        public float buffDuration = -1f;
        public int maxStacks = -1;
        public bool countdown = false;

        [Tooltip("The base object to destroy when this pickup is consumed.")]
        public GameObject baseObject;

        [Tooltip("The team filter object which determines who can pick up this pack.")]
        public TeamFilter teamFilter;

        public GameObject pickupEffect;

    }
}
