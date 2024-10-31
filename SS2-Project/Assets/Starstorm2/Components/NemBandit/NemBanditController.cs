using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SS2.Components
{
    [RequireComponent(typeof(CharacterBody))]
    public class NemBanditController : NetworkBehaviour
    {
        public CharacterBody characterBody;
        public CharacterModel characterModel;
        public void OnEnable()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        
    }
}
