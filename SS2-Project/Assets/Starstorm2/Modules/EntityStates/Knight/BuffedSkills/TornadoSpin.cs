using RoR2;

namespace EntityStates.Knight
{
    public class TornadoSpin : SpinUtility
    {
        //change the damage and effect in the esc
        //change interrupt priority from any
        //dee's concept had a spinning projectile that lingers in front of the attack at the end so do that good luck love you

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