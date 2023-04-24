using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using HG;

namespace EntityStates.CloneDrone
{
    public class CloneDroneCook : BaseSkillState
    {
        public static float duration = 4f;
        public static string spawnMuzzle = "Muzzle";
        public static float dropUpVelocityStrength = -5f;
        public static float dropForwardVelocityStrength = 3f;
        public GenericPickupController gpc;
        private ChildLocator childLocator;

        public override void OnEnter()
        {
            base.OnEnter();
            //childLocator = GetModelChildLocator();
            Debug.Log("Cooking item...");
            Debug.Log("Item tier: " + gpc.pickupIndex.pickupDef.itemTier);

            //Spawn effects
            //Multiply dur based on tier
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                //characterMotor.velocity = Vector3.zero;

                if (duration >= fixedAge)
                {
                    Debug.Log("done cooking!");
                    outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Debug.Log("exiting cook");
            PickupDropletController.CreatePickupDroplet(gpc.pickupIndex, transform.position, Vector3.up * dropUpVelocityStrength + transform.forward * dropForwardVelocityStrength);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
