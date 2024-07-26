using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke.Weapon
{
    public class ChargeLaunch : BaseNukeWeaponChargeState
    {
        public static float walkSpeedPenaltyCoefficient;

        private float _origWalkSpeedPenaltyCoefficient;

        public override void OnEnter()
        {
            base.OnEnter();
            if(characterMotor)
                _origWalkSpeedPenaltyCoefficient = characterMotor.walkSpeedPenaltyCoefficient;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (characterMotor)
                characterMotor.walkSpeedPenaltyCoefficient = walkSpeedPenaltyCoefficient;
        }

        public override void OnExit()
        {
            base.OnExit();
            if(characterMotor)
                characterMotor.walkSpeedPenaltyCoefficient = _origWalkSpeedPenaltyCoefficient;
        }
        protected override BaseNukeWeaponFireState GetFireState()
        {
            return new FireLaunch();
        }
    }
}