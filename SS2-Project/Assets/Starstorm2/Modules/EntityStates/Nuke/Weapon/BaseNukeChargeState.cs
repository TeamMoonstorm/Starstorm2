using UnityEngine;
using SS2.Components;
using SS2;
using IChargeableState = SS2.Survivors.Nuke.IChargeableState;

namespace EntityStates.Nuke
{
    /// <summary>
    /// A custom <see cref="BaseSkillState"/> that implements <see cref="IChargeableState"/>. Most of nucleator's skills start with a state that inherits from this then fires into a state that inherits <see cref="BaseNukeFireState"/>
    /// </summary>
    public abstract class BaseNukeChargeState : BaseSkillState, IChargeableState
    {
        [SerializeField, Tooltip("The starting chargeCoefficient, this is also the base damage coefficient of this skill.")]
        protected float _startingChargeCoefficient;
        [SerializeField, Tooltip("The amount of charge added to the coefficient per second, this value is multiplied by attack speed")]
        public float _baseChargeGain;
        [SerializeField, Tooltip("The coefficient at which the skill is considered overcharged.")]
        public float _chargeCoefficientSoftCap;
        [SerializeField, Tooltip("The coefficient at which the skill is fired automatically")]
        public float _chargeCoefficientHardCap;

        /// <summary>
        /// Access to the self damage controller
        /// </summary>
        public NukeSelfDamageController selfDamageController { get; private set; }

        public float currentCharge { get; protected set; } = 0.0f;

        float IChargeableState.startingChargeCoefficient => _startingChargeCoefficient;
        float IChargeableState.chargeCoefficientSoftCap => _chargeCoefficientSoftCap;

        float IChargeableState.chargeCoefficientHardCap => _chargeCoefficientHardCap;

        private float _chargeGain = 1f;
        public override void OnEnter()
        {
            base.OnEnter();
            currentCharge = _startingChargeCoefficient;
            _chargeGain = _baseChargeGain * attackSpeedStat; //Increase charge gain by attack speed
            if (gameObject.TryGetComponent<NukeSelfDamageController>(out var ctrl))
            {
                selfDamageController = ctrl;
                if (selfDamageController.isImmune) //Double charge gain if he's immune for fast attacks
                    _chargeGain *= 2f;

                //Set the default values for the controller
                selfDamageController.SetDefaults(this);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //If nucleator is charging, continue charging and report back the charge to the controller
            if (IsKeyDownAuthority())
            {
                currentCharge += _chargeGain * Time.fixedDeltaTime;
                if (selfDamageController)
                    selfDamageController.charge = currentCharge;

                //If the charge goes past the threshold, fire.
                if (currentCharge > _chargeCoefficientHardCap)
                {
                    currentCharge = _chargeCoefficientHardCap;
                    Fire();
                }
            }
            else //No longer holding down, fire.
            {
                Fire();
            }
        }

        private void Fire()
        {
            //Get the next state we should transition to
            SS2.Survivors.Nuke.IChargedState nextState = GetFireState();

            //Set the charge for the new state
            nextState.charge = currentCharge;

            //Reset damage controller
            if (selfDamageController)
            {
                selfDamageController.SetDefaults(null);
            }

            //Make sure we can cast to entity state, since IChargedState should only be applied to entity states
            bool canCastToEntityState = nextState is EntityState;
            if (!canCastToEntityState)
            {
                SS2Log.Error($"Invalid IChargedState provided by {GetType().Name}! The class implementing IChargedState should inherit from EntityState!");
                outer.SetNextStateToMain();
            }
            outer.SetNextState((EntityState)nextState);
        }

        /// <summary>
        /// Implement what state nucleator should exit to when he finishes firing.
        /// </summary>
        /// <returns>An entity state that implements <see cref="SS2.Survivors.Nuke.IChargedState"/></returns>
        protected abstract SS2.Survivors.Nuke.IChargedState GetFireState();
    }

}
