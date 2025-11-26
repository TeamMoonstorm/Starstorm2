using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.CrystalPickup
{
    public class DestroyCrystal : CrystalBaseState
    {
        public static float duration;

        public static event Action<GameObject> onPickup;
        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            onPickup?.Invoke(gameObject);

            // TO-DO: fucking fancy ! effect and that sound from returns

            Destroy(gameObject);
        }   
    }
}
