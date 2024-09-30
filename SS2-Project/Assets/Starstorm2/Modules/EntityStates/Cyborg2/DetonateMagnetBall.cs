using SS2.Components;
namespace EntityStates.Cyborg2
{
    public class DetonateMagnetBall : BaseSkillState
    {
        public static float baseDuration = 0.33f;
        private float duration;
        private ProjectileRemoteDetonator.RemoteDetonatorOwnership projectileOwnership;

        public override void OnEnter()
        {
            base.OnEnter();

            this.projectileOwnership = base.GetComponent<ProjectileRemoteDetonator.RemoteDetonatorOwnership>();
            if (!this.projectileOwnership)
            {
                this.outer.SetNextStateToMain();
                return;
            }


            //Anim
            //Sound
            this.projectileOwnership.Detonate();
            this.duration = baseDuration / this.attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            if (this.projectileOwnership)
            {
                this.projectileOwnership.Detonate();
            }
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
     
    }
}
