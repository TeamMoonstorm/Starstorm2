using MSU.Config;
using RoR2;
using SS2;
using UnityEngine;

namespace EntityStates.Knight
{
    public class TornadoSpin : SpinUtility
    {
        //change the damage and vfx in the ESC
        //dee's concept had a spinning projectile that lingers in front of the attack at the end so do that good luck love you

        public override void OnEnter()
        {
            damageCoefficient = testDamageBoosted;

            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public override void OnExit()
        {
            base.OnExit();

            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (inputBank.skill1.justPressed)
            {
                //replacement for util being interruptible by m1
                base.outer.SetNextStateToMain();
                //do projectile on exit
            }
        }
    }
}