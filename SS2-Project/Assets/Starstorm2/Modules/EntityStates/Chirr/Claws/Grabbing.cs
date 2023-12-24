using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using RoR2.Skills;
using Moonstorm.Starstorm2.Components;
using UnityEngine.Networking;

namespace EntityStates.Chirr.Claws
{
    public class Grabbing : BaseState
    {
        public static SkillDef overrideDef;
        private GrabController grabController;

        private Collider victimCollider;
        public static float releaseVictimDistance = 3f;
        public static float graceTimer = 1.5f;

        private Transform dropVisualizer;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("FullBody, Override", "GrabSuccess");
            Util.PlaySound("ChirrGrabStart", base.gameObject);
            this.grabController = base.GetComponent<GrabController>();
            if(base.isAuthority) // im fucking dogshit at netowrking man......... OnEnter happens before other clients even see the grab, so they dont see grabcontorller victim yet.
                this.victimCollider = this.grabController.victimColliders[0]; // this is always the body collider
            base.skillLocator.utility.SetSkillOverride(base.gameObject, overrideDef, GenericSkill.SkillOverridePriority.Contextual);
            this.animator = base.GetModelAnimator();
            if (this.animator)
            {
                this.animator.SetBool("inGrab", true);
            }
            this.dropVisualizer = base.FindModelChild("DropAngleCone");
            if(base.isAuthority && this.dropVisualizer)
            {
                this.dropVisualizer.gameObject.SetActive(true);
                this.dropVisualizer.transform.localScale = AimDrop.maxRayDistance * Vector3.one;
            }
            if(base.characterMotor)
            {
                base.characterMotor.Motor.ForceUnground();
            }

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            // if victim is gone or dead
            if (!base.isAuthority) return; // should be fine? grabber has "authority" over the victim's positoin

            if (this.grabController && !this.grabController.IsGrabbing())
            {
                this.outer.SetNextStateToMain();
                return;
            }
            if(!this.grabController.victimInfo.body.healthComponent.alive)
            {
                this.grabController.AttemptGrab(null);
                this.outer.SetNextStateToMain();
                return;
            }
            if (base.fixedAge < graceTimer) return;
            if(base.isGrounded)
            {
                this.grabController.AttemptGrab(null);
                this.outer.SetNextStateToMain();
                return;
            }
            Vector3 victimFootPosition = GetBetterFootPosition(); // if victim is too close to ground
            // use velocity for direction to avoid clipping into walls
            if(base.characterMotor.velocity.y < 0 && Physics.Raycast(victimFootPosition, base.characterMotor.velocity.normalized, releaseVictimDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                base.SmallHop(base.characterMotor, 9f); // hop :)
                EntityStateMachine weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon"); // throw instead of dropping
                if (weapon && weapon.state is AimDrop) weapon.SetNextStateToMain();
                else
                {
                    this.grabController.AttemptGrab(null);
                }
                this.outer.SetNextStateToMain();
                return;
            }

        }

        public Vector3 GetBetterFootPosition()
        {
            Vector3 position = this.grabController.victimBodyObject.transform.position;
            if (this.grabController.victimInfo.characterMotor)
            {
                position = this.grabController.victimInfo.body.footPosition;
                return position;
            }
            if(this.victimCollider) // lowest point on collider (assuming its centered)
            {
                position.y = this.victimCollider.bounds.min.y;
                return position;
            }
            return position;
        }
        public override void OnExit()
        {
            base.OnExit();
            if (this.animator)
            {
                this.animator.SetBool("inGrab", false); // transitions from GrabLoop -> GrabRelease when false
            }
            if (base.isAuthority && this.grabController && this.grabController.IsGrabbing())
            {
                this.grabController.AttemptGrab(null);
            }
            if (base.isAuthority && this.dropVisualizer)
            {
                this.dropVisualizer.gameObject.SetActive(false);
            }
            base.skillLocator.utility.UnsetSkillOverride(base.gameObject, overrideDef, GenericSkill.SkillOverridePriority.Contextual);
        }
    }
}
