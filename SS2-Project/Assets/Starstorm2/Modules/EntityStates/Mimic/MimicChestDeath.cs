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
        bool shouldMakeVFX = true;

        MimicInventoryManager mimicInventory;

        public override bool shouldAutoDestroy => false;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "BufferEmpty");

            if(characterBody.modelLocator.modelTransform.TryGetComponent<CharacterModel>(out var cmodel))
            {
                if(cmodel.invisibilityCount >= 1)
                {
                    shouldMakeVFX = false;
                }
            }

            //Replicating the chest opening VFX
            var zipL = FindModelChild("ZipperL");
            if (zipL && shouldMakeVFX)
            {
                lVFX = UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.zipperVFX, zipL);
            }

            var zipR = FindModelChild("ZipperR");
            if (zipR && shouldMakeVFX)
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

            mimicInventory = gameObject.GetComponent<MimicInventoryManager>();

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //Attempting to lock mimic in place when it dies
            characterMotor.velocity.x = 0;
            characterMotor.velocity.z = 0;

            if (fixedAge > 1.5f && !hasDropped && mimicInventory)
            {
                hasDropped = true;
                mimicInventory.DropItems();
            }
            if (!shouldMakeVFX && !hasDropped && mimicInventory)
            {
                hasDropped = true;
                mimicInventory.DropItems();
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
