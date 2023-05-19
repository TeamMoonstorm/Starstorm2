using RoR2;
using UnityEngine;

namespace EntityStates.Executioner2
{
    public class AimIonGunOld : BaseSkillState
    {
        public static float baseDuration = 0.1f;

        private float chargeTimer = 0f;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            //Debug.Log("entering");
            duration = baseDuration / characterBody.attackSpeed;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (duration >= fixedAge && isAuthority /*&& !inputBank.skill2.down*/)
            {
                //Debug.Log("over duration");
                PlayAnimation("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration);
                FireIonGunOld nextState = new FireIonGunOld();
                nextState.activatorSkillSlot = activatorSkillSlot;
                outer.SetNextState(nextState);
            }
            /*chargeTimer += Time.fixedDeltaTime;
            if (chargeTimer >= 1f)
            {
                Debug.Log("chargetimer >= 1f");
                Util.PlaySound("ExecutionerGainCharge", gameObject);
                skillLocator.secondary.AddOneStock();
                chargeTimer = 0f;
            }*/
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }

}