using UnityEngine;
namespace EntityStates.AcidBug
{
    public class HurtState : HurtStateFlyer
    {

        public override void OnEnter()
        {
            base.OnEnter();

            if (rigidbodyMotor)
            {
                rigidbodyMotor.moveVector = Vector3.zero;
            }    
        }
    }
}
