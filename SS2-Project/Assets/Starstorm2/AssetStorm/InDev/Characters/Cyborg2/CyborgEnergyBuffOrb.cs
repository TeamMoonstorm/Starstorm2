using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2.Orbs;
using RoR2;
using UnityEngine;
using EntityStates.Cyborg2;
namespace Moonstorm.Starstorm2.Components
{
    public class CyborgEnergyBuffOrb : Orb
    {
        public float speed = 90f;
        public override void Begin()
        {
            //this.duration = this.distanceToTarget / speed;

            GameObject effectPrefab = SS2Assets.LoadAsset<GameObject>("ZapperBuffOrbEffect", SS2Bundle.Indev);          
            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };
            effectData.SetHurtBoxReference(this.target);
            EffectManager.SpawnEffect(effectPrefab, effectData, true);
        }
        public override void OnArrival()
        {
            if (!this.target) return;

            SkillLocator skillLocator = this.target.healthComponent.body.skillLocator;


            //DIPSHITTTTTTTTTTTTTTTTTTTTTT
            EntityStateMachine weapon = EntityStateMachine.FindByCustomName(this.target.healthComponent.gameObject, "Weapon");
            if (weapon && !weapon.SetInterruptState(new CatchZapper(), EntityStates.InterruptPriority.Any))
            {
                // this is stupid
                GenericSkill skill = skillLocator.FindSkill("SecondaryCharged");
                if (skill)
                {
                    skill.AddOneStock();
                }
                CharacterMotor motor = this.target.healthComponent.body.characterMotor;
                if(motor)
                {
                    if(!motor.isGrounded)
                        motor.velocity = new Vector3(motor.velocity.x, Mathf.Max(motor.velocity.y, CatchZapper.smallHopVelocity), motor.velocity.z);
                }
            }
        }
    }
}
