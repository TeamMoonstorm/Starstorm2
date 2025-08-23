using UnityEngine;
using UnityEngine.Serialization;

namespace EntityStates.Generic
{
    public abstract class BaseWindDownState : BaseState
    {
        [SerializeField]
        public float baseDuration;
        [SerializeField]
        public InterruptPriority minimumInterruptPriority;
        [SerializeField]
        public bool ignoreAttackSpeed;

        protected float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = ignoreAttackSpeed? baseDuration : baseDuration / attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;

            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return minimumInterruptPriority;
        }
    }
}