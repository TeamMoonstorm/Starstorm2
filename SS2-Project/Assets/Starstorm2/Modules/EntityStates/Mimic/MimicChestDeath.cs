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

        public override bool shouldAutoDestroy => false;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "BufferEmpty");

            //Replicating the chest opening VFX
            if (NetworkServer.active)
            {
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
            }

            //Attempting to lock mimic in place when it dies
            rigidbody.velocity = Vector3.zero;
            characterMotor.velocity = Vector3.zero;

            if (gameObject)
            {
               var mim = gameObject.GetComponent<MimicInventoryManager>();
               if (mim)
               {
                   mim.BeginDropCountdown();
               }
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

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();

            Destroy(lVFX);
            Destroy(rVFX);
        }
    }
}
