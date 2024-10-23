using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class KnightPassiveController : NetworkBehaviour
    {
        public CharacterBody characterBody;
        public GameObject PassiveWardPrefab = SS2.Survivors.Knight.KnightPassiveWard;

        public void Start()
        {
            //characterBody = GetComponent<CharacterBody>();
            if (NetworkServer.active)
            {
                GameObject passiveWardInstance = UnityEngine.Object.Instantiate(PassiveWardPrefab, characterBody.footPosition, Quaternion.identity);
                passiveWardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
                characterBody.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(passiveWardInstance);
            }
        }
    }
}
