using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using EntityStates;


namespace EntityStates.NemMerc
{
    public class Reload : BaseState
    {
        public static float baseDuration = 1f;
        public static string reloadSoundString;

        public float reloadTime = 0.5f;

        private float duration;
        private bool hasReloaded;

        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = Reload.baseDuration / this.attackSpeedStat;
            //anim

            /// NEED TO SYNC SOUNDS AN ANIMATIONS
            /// MAYBE JUST USE ANIMATION EVENTS FOR RELOAD SOUNDS
            /// ALSO SHELLS
            base.PlayCrossfade("Gesture, Override", "Reload2", "Reload.playbackRate", this.duration * 0.9f, 0.1f);
            Util.PlaySound(reloadSoundString, base.gameObject);
            //vfx
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= this.duration * this.reloadTime && !this.hasReloaded)
            {
                this.hasReloaded = true;
                this.AddStock();
                //sound
                
            }

            if(base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void AddStock()
        {
            base.skillLocator.primary.stock = 2;

            //sound
            //vfx
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return this.hasReloaded ? InterruptPriority.Any : InterruptPriority.Skill;
        }
    }
}
