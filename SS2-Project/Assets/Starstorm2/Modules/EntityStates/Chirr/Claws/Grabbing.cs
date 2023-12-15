using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using RoR2.Skills;
using Moonstorm.Starstorm2.Components;
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
        public override void OnEnter()
        {
            base.OnEnter();
            //anim
            //sound
            this.grabController = base.GetComponent<GrabController>();
            this.victimCollider = this.grabController.victimColliders[0]; // this is always the body collider
            base.skillLocator.utility.SetSkillOverride(base.gameObject, overrideDef, GenericSkill.SkillOverridePriority.Contextual);

            this.dropVisualizer = base.FindModelChild("DropAngleCone");
            if(base.isAuthority && this.dropVisualizer)
            {
                this.dropVisualizer.gameObject.SetActive(true);
                this.dropVisualizer.transform.localScale = AimDrop.maxRayDistance * Vector3.one;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            // if victim is gone or dead
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
            Vector3 victimFootPosition = GetBetterFootPosition(); // if victim is too close to ground
            if(Physics.Raycast(victimFootPosition, Vector3.down, releaseVictimDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                
                EntityStateMachine weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon"); // throw instead of dropping
                if (weapon && weapon.state is AimDrop) weapon.SetNextStateToMain();
                else this.grabController.AttemptGrab(null);
                this.outer.SetNextStateToMain();
                return;
            }

        }

        public Vector3 GetBetterFootPosition()
        {
            Vector3 position = this.grabController.victimBodyObject.transform.position;
            if (this.characterMotor)
            {
                position.y -= this.characterMotor.capsuleHeight * 0.5f;
                return position;
            }
            if(this.victimCollider) // lowest point on collider (assuming its centered)
            {
                position.y -= this.victimCollider.bounds.min.y * 0.5f;
                return position;
            }
            return position;
        }
        public override void OnExit()
        {
            base.OnExit();
            if (base.isAuthority && this.dropVisualizer)
            {
                this.dropVisualizer.gameObject.SetActive(false);
            }
            base.skillLocator.utility.UnsetSkillOverride(base.gameObject, overrideDef, GenericSkill.SkillOverridePriority.Contextual);
        }
    }
}
