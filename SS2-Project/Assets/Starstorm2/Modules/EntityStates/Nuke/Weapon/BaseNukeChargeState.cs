using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using MSU;
using SS2.Components;
using SS2;

namespace EntityStates.Nuke
{
    public abstract class BaseNukeChargeState : BaseSkillState, SS2.Survivors.Nuke.IChargeableState
    {
        [SerializeField, Tooltip("The starting chargeCoefficient, this is also the base damage coefficient of this skill.")]
        protected float _startingChargeCoefficient;
        [SerializeField, Tooltip("The amount of charge added to the coefficient per second, this value is multiplied by attack speed")]
        protected float _baseChargeGain;
        [SerializeField, Tooltip("The coefficient at which the skill is considered overcharged.")]
        protected float _chargeCoefficientSoftCap;
        [SerializeField, Tooltip("The coefficient at which the skill is fired automatically")]
        protected float _chargeCoefficientHardCap;

        public NukeSelfDamageController SelfDamageController { get; private set; }

        public float currentCharge { get; protected set; }

        public float chargeCoefficientSoftCap => _chargeCoefficientSoftCap;

        public float chargeCoefficientHardCap => _chargeCoefficientHardCap;

        private float chargeGain;
        public override void OnEnter()
        {
            base.OnEnter();
            currentCharge = _startingChargeCoefficient;
            chargeGain = _baseChargeGain * attackSpeedStat;
            if(gameObject.TryGetComponent<NukeSelfDamageController>(out var ctrl))
            {
                SelfDamageController = ctrl;
                if (SelfDamageController.isImmune)
                    chargeGain *= 2f;
                SelfDamageController.AsValidOrNull()?.SetDefaults(this);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(IsKeyDownAuthority())
            {
                currentCharge += chargeGain * Time.fixedDeltaTime;
                if (SelfDamageController)
                    SelfDamageController.Charge = currentCharge;

                if(currentCharge > _chargeCoefficientHardCap)
                {
                    currentCharge = _chargeCoefficientHardCap;
                    Fire();
                }
            }
            else
            {
                Fire();
            }
        }

        private void Fire()
        {
            SS2.Survivors.Nuke.IChargedState nextState = GetFireState();
            nextState.charge = currentCharge;

            if(SelfDamageController)
            {
                SelfDamageController.SetDefaults(null);
            }

            bool canCastToEntityState = nextState is EntityState;
            if(!canCastToEntityState)
            {
                SS2Log.Error($"Invalid IChargedState provided by {GetType().Name}! The class implementing IChargedState should inherit from EntityState!");
                outer.SetNextStateToMain();
            }
            outer.SetNextState((EntityState)nextState);
        }
        protected abstract SS2.Survivors.Nuke.IChargedState GetFireState();
    }

}
