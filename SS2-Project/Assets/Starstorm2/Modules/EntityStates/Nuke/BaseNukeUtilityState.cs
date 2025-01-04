using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.Nuke
{
    /// <summary>
    /// Base state when nucleator fires either of his utilities.
    /// </summary>
    public abstract class BaseNukeUtilityState : GenericCharacterMain, SS2.Survivors.Nuke.IChargedState
    {
        [Tooltip("The min launch speed this utility has, final launch speed is derived from the charge value given by its corresponding charging state")]
        [SerializeField] public float minLaunchSpeed;
        [Tooltip("The max launch speed this utility has, final launch speed is derived from the charge value given by its corresponding charging state")]
        [SerializeField] public float maxLaunchSpeed;
        [Tooltip("The base duration of this state")]
        [SerializeField] public float baseDuration;

        protected float _launchSpeed;
        protected Vector3 _velocity;
        private float _duration;

        public float charge { get; set; }

        public override void OnEnter()
        {
            base.OnEnter();
            //Unground the boy and calculate his velocity based off his charge
            if (isAuthority && characterMotor)
            {
                characterMotor.Motor.ForceUnground();
                _velocity = CalculateLaunchVelocity(characterMotor.velocity, GetAimRay().direction, charge, minLaunchSpeed, maxLaunchSpeed);
                characterMotor.velocity = _velocity;
                characterDirection.forward = _velocity.normalized;
                _launchSpeed = _velocity.magnitude;
                _duration = baseDuration;
            }
        }

        public override void FixedUpdate()
        {
            //always sprint, try exit whenever possible.
            base.FixedUpdate();
            if (isAuthority)
            {
                characterBody.isSprinting = true;
            }
            TryExitState();
        }

        protected virtual void TryExitState()
        {
            if (fixedAge > _duration)
                outer.SetNextStateToMain();
        }

        protected virtual Vector3 CalculateLaunchVelocity(Vector3 currentVelocity, Vector3 aimDirection, float charge, float minLaunchSpeed, float maxLaunchSpeed)
        {
            currentVelocity = ((Vector3.Dot(currentVelocity, aimDirection) < 0f) ? Vector3.zero : Vector3.Project(currentVelocity, aimDirection));
            return currentVelocity + aimDirection * Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, charge);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}