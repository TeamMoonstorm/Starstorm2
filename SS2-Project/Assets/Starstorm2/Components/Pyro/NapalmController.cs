using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SS2.Components
{
    public class NapalmController : NetworkBehaviour
    {
        public Transform FX;
        public AlignToNormal atn;

        public void Start()
        {
            RaycastHit hit;
                                                                                    // floating fire 
            if (!Physics.Raycast(transform.position, Vector3.down, out hit, 2.5f)) // golden wire
            {                                                                     // silver trails
                Destroy(this.gameObject);                                        // fishing spire
                return;                                                         // but none of that here
            }

            transform.rotation.Set(0f, 0f, 0f, 0f);
            FX.transform.rotation.Set(0f, 0f, 0f, 0f); // STOP

            atn.DoAlignToNormal();
        }
    }
}
