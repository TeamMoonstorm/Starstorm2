using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class KnightPassiveController : NetworkBehaviour
    {
        public CharacterBody characterBody;
        public GameObject PassiveWardPrefab;

        public void Start()
        {
            if (NetworkServer.active && characterBody != null && PassiveWardPrefab != null)
            {
                GameObject passiveWardInstance = UnityEngine.Object.Instantiate(PassiveWardPrefab, characterBody.footPosition, Quaternion.identity);
                passiveWardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                passiveWardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(characterBody.gameObject);
            }
        }
    }
}
