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

namespace EntityStates.Nuke.Weapon
{
    public abstract class BaseNukeWeaponChargeState : BaseSkillState
    {
        [SerializeField, Tooltip("The starting chargeCoefficient, this is also the base damage coefficient of this skill.")]
        public float startingChargeCoefficient;
        [SerializeField, Tooltip("The amount of charge added to the coefficient per second, this value is multiplied by attack speed")]
        public float baseChargeGain;
        [SerializeField, Tooltip("The coefficient at which the skill is considered overcharged.")]
        public float chargeCoefficientSoftCap;
        [SerializeField, Tooltip("The coefficient at which the skill is fired automatically")]
        public float chargeCoefficientHardCap;

        public NukeSelfDamageController SelfDamageController { get; private set; }
        public float CurrentCharge { get; protected set; }
        private float chargeGain;
        public override void OnEnter()
        {
            base.OnEnter();
            CurrentCharge = startingChargeCoefficient;
            chargeGain = baseChargeGain * attackSpeedStat;
            if(gameObject.TryGetComponent<NukeSelfDamageController>(out var ctrl))
            {
                SelfDamageController = ctrl;
                if (SelfDamageController.IsImmune)
                    chargeGain *= 2f;
                SelfDamageController.AsValidOrNull()?.SetDefaults(this);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(IsKeyDownAuthority())
            {
                CurrentCharge += chargeGain * Time.fixedDeltaTime;
                if (SelfDamageController)
                    SelfDamageController.Charge = CurrentCharge;

                if(CurrentCharge > chargeCoefficientHardCap)
                {
                    CurrentCharge = chargeCoefficientHardCap;
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
            BaseNukeWeaponFireState nextState = GetFireState();
            nextState.Charge = CurrentCharge;

            if(SelfDamageController)
            {
                SelfDamageController.SetDefaults(weaponState: null);
            }

            outer.SetNextState(nextState);
        }
        protected abstract BaseNukeWeaponFireState GetFireState();
    }

}
