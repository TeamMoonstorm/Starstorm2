using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using RoR2.Skills;
using System;
using EntityStates.Cyborg2.Teleporter;
namespace Moonstorm.Starstorm2.Components
{
    public class TeleporterTurret : MonoBehaviour
    {
        public ProjectileController controller;
        private CharacterBody ownerBody;
        public EntityStateMachine machine;
        public float activationRange = 14f;
        private List<CharacterBody> trackedBodies = new List<CharacterBody>();
        void Start()
        {
            this.controller = base.GetComponentInParent<ProjectileController>();

            SphereCollider collider = base.GetComponent<SphereCollider>();
            collider.radius = this.activationRange;

            if (this.controller)
            {
                GameObject owner = this.controller.owner;
                if(owner)
                {
                    this.ownerBody = owner.GetComponent<CharacterBody>();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterBody body = other.GetComponent<CharacterBody>();
            if(!body) // IDK XD XD XD XD XD XD XD DJASH MANDG YAUWRuidhkasfj
            {
                HurtBox hurtBox = other.GetComponent<HurtBox>();
                if(hurtBox)
                {
                    body = hurtBox.healthComponent.body;
                }
            }
            if(body && body.teamComponent.teamIndex == this.controller.teamFilter.teamIndex && !trackedBodies.Contains(body))
            {
                trackedBodies.Add(body);
                Chat.AddMessage(body.ToString() + "+");
                body.onSkillActivatedServer += FireTurret;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            CharacterBody body = other.GetComponent<CharacterBody>();
            if (!body)
            {
                HurtBox hurtBox = other.GetComponent<HurtBox>();
                if (hurtBox)
                {
                    body = hurtBox.healthComponent.body;
                }
            }
            if (trackedBodies.Contains(body))
            {
                Chat.AddMessage(body.ToString() + "-");
                trackedBodies.Remove(body);
                body.onSkillActivatedServer -= FireTurret;
            }
        }


        private void OnDestroy()
        {
            foreach(CharacterBody body in this.trackedBodies)
            {
                body.onSkillActivatedServer -= FireTurret;
            }
        }
        private void FireTurret(GenericSkill skill)
        {
            Debug.Log("skill activate d:)");
            if(this.machine)
            {
                this.machine.SetInterruptState(new FireTurret { owner = this.ownerBody, activatorBody = skill.characterBody }, EntityStates.InterruptPriority.Any);
                Debug.Log("turret shooty");
            }
        }

        
        
    }
}

