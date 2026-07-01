using RoR2.UI;
using SS2;
using SS2.Components;
using SS2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Starstorm2.Components
{
    public class BirthrightObjectiveTimer : NetworkBehaviour
    {
        [SyncVar] 
        public float timer;
        public bool spawnedMeteors;

        private void OnEnable()
        {
            PrimalBirthright.Behavior.timerComponent = this;
            if (PrimalBirthrightObjectiveToken.instanceList.Count > 0) // dont add objective on first pickup when no chests exist ,.. 
            {
                ObjectivePanelController.collectObjectiveSources += PrimalBirthright.OnCollectObjectiveSources;
            }
            
            if (!NetworkServer.active) return;
            timer = PrimalBirthright.birthrightCompletionTime + (PrimalBirthright.birthrightCompletionTimeStacking * (RoR2.Util.GetItemCountGlobal(SS2Content.Items.PrimalBirthright.itemIndex, false) - 1));
        }

        private void OnDisable()
        {
            ObjectivePanelController.collectObjectiveSources -= PrimalBirthright.OnCollectObjectiveSources;
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;
            
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                return;
            }

            if (spawnedMeteors) return;
            
            RpcSpawnMeteors();
            SpawnMeteors();
        }

        [ClientRpc]
        public void RpcSpawnMeteors()
        {
            SS2Log.Debug("running spawn meteros rpc");
            SpawnMeteors();
        }

        private void SpawnMeteors()
        {
            spawnedMeteors = true;
            gameObject.GetComponent<BirthrightMeteorStormController>().enabled = true;
            gameObject.GetComponent<ChildLocator>().FindChild("PPin").gameObject.SetActive(true);
            SS2Log.Debug("spawning meteors !!");
        }

        public void TryRemoveObjective()
        {
            if (!spawnedMeteors)
            {
                ObjectivePanelController.collectObjectiveSources -= PrimalBirthright.OnCollectObjectiveSources;
            }
        }
    }
}