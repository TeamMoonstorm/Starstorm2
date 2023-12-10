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
        public override void OnEnter()
        {
            base.OnEnter();
            //anim
            //sound
            this.grabController = base.GetComponent<GrabController>();
            base.skillLocator.utility.SetSkillOverride(base.gameObject, overrideDef, GenericSkill.SkillOverridePriority.Contextual);
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
        }
        public override void OnExit()
        {
            base.OnExit();
            base.skillLocator.utility.UnsetSkillOverride(base.gameObject, overrideDef, GenericSkill.SkillOverridePriority.Contextual);
        }
    }
}
