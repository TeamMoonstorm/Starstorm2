using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    [RequireComponent(typeof(CharacterBody))]
    public class DukeController : NetworkBehaviour
    {
        public bool useAltCamera;

        private void Awake()
        {
            // TODO Make it configurable
            useAltCamera = false;
        }
    }
}
