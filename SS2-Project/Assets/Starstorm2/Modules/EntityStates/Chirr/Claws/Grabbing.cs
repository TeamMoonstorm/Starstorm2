using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using Moonstorm.Starstorm2.Components;
namespace EntityStates.Chirr.Claws
{
    public class Grabbing : BaseState
    {
        private GrabController grabController;
        private EntityStateMachine weapon;
        public override void OnEnter()
        {
            base.OnEnter();
            this.grabController = base.GetComponent<GrabController>();
            weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");

            Chat.AddMessage("HAHAHAHAHAHA LOl");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            // if victim is gone or dead
            //if(this.grabController && (!this.grabController.victimBodyObject || (this.grabController.victimBodyObject && this.grabController.victimInfo.body && this.grabController.victimInfo.body.healthComponent.alive)))
            //{
            //    this.outer.SetNextStateToMain();
            //    return;
            //}

            if(base.inputBank.skill3.justPressed) // FUCKING TERRIBLE. USE SKILL OVERRIDE OR SOMETHING ASAP!!!!!!!!!!!!!!
            {               
                if(weapon && weapon.SetInterruptState(new AimDrop(), InterruptPriority.Skill))
                {
                    Chat.AddMessage("WEEEEEEEEEEEEEEEEEEEEEE");
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
        }
    }
}
