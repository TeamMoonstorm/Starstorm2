using RoR2;
using SS2;
using SS2.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Mimic
{
    public class MimicChestDeath : GenericCharacterDeath
    {

        GameObject lVFX;
        GameObject rVFX;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "BufferEmpty");
            //RoR2/Base/Chest1/ChestUnzip.prefab
            //EffectManager.SpawnEffect()

            //string effectName = "RoR2/Base/Chest1/ChestUnzip.prefab";
            //var effect = Addressables.LoadAssetAsync<GameObject>(effectName).WaitForCompletion();

            //SS2.Monsters.Mimic.zipperVFX
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

            rigidbody.velocity = Vector3.zero;

            if (gameObject)
            {
               var mim = gameObject.GetComponent<MimicInventoryManager>();
               SS2Log.Error("Death enter : " + mim + " | ");
               if (mim)
               {
                   SS2Log.Error("calling : " + mim + " | ");
                   mim.BeginCountdown();
               }
            }

            //var z1 = FindModelChild("Zipper1");
            //var z2 = FindModelChild("Zipper2");
            //if (z1)
            //{
            //    UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.zipperVFX, z1.position, Quaternion.identity);
            //}
            //if (z2)
            //{
            //    UnityEngine.Object.Instantiate<GameObject>(SS2.Monsters.Mimic.zipperVFX, z2.position, Quaternion.identity);
            //}

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            SS2Log.Error("Death Exit : " + gameObject);
            Destroy(lVFX);
            Destroy(rVFX);
            base.OnExit();
        }
    }
}
