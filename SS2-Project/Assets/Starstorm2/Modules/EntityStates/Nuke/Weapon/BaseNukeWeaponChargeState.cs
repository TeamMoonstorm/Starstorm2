using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using Moonstorm;
using Moonstorm.Starstorm2.Components;
using Moonstorm.Starstorm2;

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
#if DEBUG
        public string TypeName { get; private set; }
#endif
        private float chargeGain;
        public override void OnEnter()
        {
            base.OnEnter();
            SelfDamageController = GetComponent<NukeSelfDamageController>();
            CurrentCharge = startingChargeCoefficient;
            chargeGain = baseChargeGain * attackSpeedStat;
            if (SelfDamageController.IsImmune)
                chargeGain *= 2f;
#if DEBUG
            TypeName = GetType().Name;
#endif
            SelfDamageController.AsValidOrNull()?.SetDefaults(this);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(IsKeyDownAuthority())
            {
#if DEBUG
                var oldCharge = CurrentCharge;
                CurrentCharge += chargeGain * Time.fixedDeltaTime;
                SS2Log.Info($"{TypeName} previousCharge: {oldCharge} - newCharge: {CurrentCharge}");
#else
                CurrentCharge += chargeGain * Time.fixedDeltaTime;
#endif
                if (SelfDamageController)
                    SelfDamageController.Charge = CurrentCharge;

                if(CurrentCharge > chargeCoefficientHardCap)
                {
                    CurrentCharge = chargeCoefficientHardCap;
#if DEBUG
                    SS2Log.Info($"{TypeName} hard cap reached. currentCharge: {CurrentCharge}, hardCap: {chargeCoefficientHardCap}");
#endif
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
