using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class KnightPassiveController : NetworkBehaviour
    {
        public CharacterBody characterBody;

        [Header("Passive Ward Prefab")]
        [SerializeField]
        public GameObject PassiveWardPrefab;

        public void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            if (NetworkServer.active)
            {
                GameObject passiveWardInstance = UnityEngine.Object.Instantiate(PassiveWardPrefab, characterBody.footPosition, Quaternion.identity);
                passiveWardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                characterBody.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(passiveWardInstance);
            }
        }

        private void OnDisable()
        {
        }

    }
}
