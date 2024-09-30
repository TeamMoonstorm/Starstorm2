using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Starstorm2.Modules.EntityStates.Executioner2.Templar
{
    public class Consecration : BaseSkillState
    {
        public float baseDuration = 0.5f;
        public static BuffDef holyBuff;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.AddTimedBuff(holyBuff, 10f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
