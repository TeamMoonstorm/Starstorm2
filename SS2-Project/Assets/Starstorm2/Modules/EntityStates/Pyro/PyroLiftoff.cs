using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Pyro
{
    public class PyroLiftoff : GenericCharacterMain
    {
        public static float duration = 0.2f;
        public static AnimationCurve speedCoefficientCurve;
        private Vector3 flyVector = Vector3.zero;

        public override void OnEnter()
        {
            base.OnEnter();
            flyVector = Vector3.up;
            characterMotor.Motor.ForceUnground();
            characterMotor.velocity = Vector3.zero;

            if (NetworkServer.active)
            {
                characterBody.AddBuff(SS2.Survivors.Pyro._bdPyroJet);
            }
        }

        public override void HandleMovements()
        {
            base.HandleMovements();
            characterMotor.rootMotion += flyVector * (moveSpeedStat * speedCoefficientCurve.Evaluate(fixedAge / duration) * GetDeltaTime());
            characterMotor.velocity.y = 0f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((fixedAge >= duration) && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
