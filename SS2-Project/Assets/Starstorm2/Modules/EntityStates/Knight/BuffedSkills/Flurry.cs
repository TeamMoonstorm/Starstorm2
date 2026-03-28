using MSU.Config;
using RoR2;
using SS2;
using UnityEngine;

namespace EntityStates.Knight
{
    public class Flurry : BaseKnightMeleeAttack
    {
        public float totalDuration { get; set; }
        private bool swipeDown;
        private static Vector3 swipeUpForce = new Vector3 (-8f, 5f, 3f);
        private static Vector3 swipeDownForce = new Vector3(8f, -3f, 3f);

        public override void OnEnter()
        {
            baseDuration = SwordDashToFlurry.flurrySwipeDuration;
            damageCoefficient = SwordDashToFlurry.flurrySwipeDamage;

            muzzleString = (swipeDown ? "SwingDownMuzzle" : "SwingUpMuzzle") + Random.Range(1,4);

            base.OnEnter();
            this.attack.damageType.damageSource = DamageSource.Primary;
            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public override void OnExit()
        {
            base.OnExit();

            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }


        public override void PlayAttackAnimation()
        {
            PlayAnimation("FullBody, Override", "FlurrySwipe" + (swipeDown ? "Down" : "Up"));
        }
        protected override void AuthorityModifyOverlapAttack(OverlapAttack attack)
        {
            base.AuthorityModifyOverlapAttack(attack);

            attack.forceVector = Util.QuaternionSafeLookRotation(characterDirection.forward) * (swipeDown ? swipeDownForce : swipeUpForce);
            attack.physForceFlags = PhysForceFlags.resetVelocity | PhysForceFlags.massIsOne | PhysForceFlags.ignoreGroundStick | PhysForceFlags.disableAirControlUntilCollision;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterMotor.velocity = Vector3.zero;
        }

        protected override void SetNextState()
        {
            if(totalDuration > 0)
            {
                outer.SetNextState(new Flurry { totalDuration = totalDuration - fixedAge, swipeDown = !swipeDown});
            }
            else
            {
                outer.SetNextStateToMain();
            }
        }

    }
}