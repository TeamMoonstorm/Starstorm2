using SS2.Items;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Components
{
    class SoulPickup : MonoBehaviour
    {
        public PickupDropTable dropTable;
        public GameObject baseObject;
        public GameObject effectPrefab;
        public TeamFilter team;
        public float chance;
        private bool alive = true;
        private void OnTriggerStay(Collider other)
        {
            if (NetworkServer.active && alive && TeamComponent.GetObjectTeam(other.gameObject) == team.teamIndex)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body)
                {
                    alive = false;
                    EffectManager.SimpleEffect(effectPrefab, base.transform.position, Quaternion.identity, true);
                    if (Util.CheckRoll(chance))
                    {
                        PickupIndex pickupIndex = dropTable.GenerateDrop(new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong));
                        if(pickupIndex != PickupIndex.none)
                            PickupDropletController.CreatePickupDroplet(pickupIndex, base.transform.position, new Vector3(0, 15, 0));
                    }
                    Destroy(baseObject);
                }
            }
        }
    }
}