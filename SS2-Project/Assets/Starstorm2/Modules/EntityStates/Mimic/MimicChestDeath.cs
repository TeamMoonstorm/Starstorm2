using KinematicCharacterController;
using RoR2;
using RoR2.Hologram;
using SS2;
using SS2.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.Mimic
{
    public class MimicChestDeath : GenericCharacterDeath
    {
        GameObject lVFX;
        GameObject rVFX;
        bool hasDropped = false;
        
        public override bool shouldAutoDestroy => false;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "BufferEmpty");

            //Replicating the chest opening VFX
            var zipL = FindModelChild("ZipperL");
            if (zipL)
            {
                lVFX = UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.zipperVFX, zipL);
            }

            var zipR = FindModelChild("ZipperR");
            if (zipR)
            {
                rVFX = UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.zipperVFX, zipR);
            }

            var purchaseInter = GetComponent<PurchaseInteraction>();
            if (NetworkServer.active && purchaseInter)
            {
                purchaseInter.SetAvailable(false);
            }
            var holo = GetComponent<HologramProjector>();
            if (holo)
            {
                holo.enabled = false;
            }

            //Let them smush into the ground more
            var kinematic = GetComponent<KinematicCharacterMotor>();
            kinematic.SetCapsuleDimensions(kinematic.CapsuleRadius, kinematic.CapsuleHeight, .925f);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //Attempting to lock mimic in place when it dies
            characterMotor.velocity.x = 0;
            characterMotor.velocity.z = 0;
            var mim = gameObject.GetComponent<MimicInventoryManager>();

            if (fixedAge > 1.5f && !hasDropped && mim)
            {
                hasDropped = true;
                mim.DropItems();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (lVFX)
            {
                GameObject.Destroy(lVFX);
            }
            
            if (rVFX)
            {
                GameObject.Destroy(rVFX);
            }
 
        }
    }
}
