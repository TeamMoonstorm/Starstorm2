using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
    public class GravitateArrowPickup : MonoBehaviour
    {
        private Transform gravitateTarget;

        [Tooltip("The rigidbody to set the velocity of.")]
        public Rigidbody rb;
        [Tooltip("The TeamFilter of the team that can activate this trigger.")]
        public TeamFilter teamFilter;
        [Tooltip("The CharacterBody which can activate this trigger.")]
        public CharacterBody characterBody;

        public float acceleration;
        public float maxSpeed;

        private void Start()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            if (NetworkServer.active && !gravitateTarget && teamFilter.teamIndex != TeamIndex.None)
            {
                CharacterBody characterBody = other.gameObject.GetComponent<CharacterBody>();
                if (TeamComponent.GetObjectTeam(other.gameObject) == teamFilter.teamIndex && characterBody.bodyIndex == this.characterBody.bodyIndex)
                {
                    SkillLocator skillLocator = characterBody.skillLocator;
                    if (skillLocator.primary.stock < 7)
                        gravitateTarget = other.gameObject.transform;
                }
            }
        }

        private void FixedUpdate()
        {
            if (gravitateTarget)
            {
                rb.velocity = Vector3.MoveTowards(rb.velocity, (gravitateTarget.transform.position - transform.position).normalized * maxSpeed, acceleration);
            }
        }
    }
}
