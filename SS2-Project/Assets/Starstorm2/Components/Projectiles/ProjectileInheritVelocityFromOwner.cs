using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace SS2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileInheritVelocityFromOwner : MonoBehaviour
    {
        private Rigidbody rb;
        private ProjectileController proj;
        private Vector3 inheritance;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            proj = GetComponent<ProjectileController>();
        }

        private void Start()
        {
            // no owner = no velocity
            if (proj.owner == null)
            {
                inheritance = Vector3.zero;
                return;
            }

            else
            {
                // first try character motor
                if (proj.owner.TryGetComponent(out CharacterBody body) && body.characterMotor != null)
                {
                    inheritance = body.characterMotor.velocity;
                    return;
                }

                // rigidbody if that failed
                else if (proj.owner.TryGetComponent(out Rigidbody rb))
                {
                    inheritance = rb.velocity;
                    return;
                }
            }

            // fuck it, nothing
            inheritance = Vector3.zero;
        }

        private void FixedUpdate()
        {
            // add inherited velocity to rigidbody
            rb.velocity += inheritance;
        }
    }
}
