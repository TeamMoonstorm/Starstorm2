using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;

namespace EntityStates.NemMerc
{
    public class PrepRetaliate : BaseSkillState
    {
        public static float baseDuration = 0.33f;
        private float duration;
        public float damageReceived;
        private EntityStateMachine weapon;
        public override void OnEnter()
        {
            base.OnEnter();
            //vfx
            //sound
            Util.PlaySound("Play_commando_M2_grenade_throw", base.gameObject);
            //anim
            this.duration = PrepRetaliate.baseDuration / this.attackSpeedStat;
            this.weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.characterMotor)
                base.characterMotor.velocity = Vector3.zero;

            if(base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                if(this.weapon)
                {
                    this.weapon.SetInterruptState(new Retaliate(), InterruptPriority.PrioritySkill);
                }
            }
        }
    }
}
