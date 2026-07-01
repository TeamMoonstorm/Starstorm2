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
        [SyncVar] 
        public bool completed;
        
        public bool spawnedMeteors;

        private void OnEnable()
        {
            if (NetworkServer.active)
            {
                timer = PrimalBirthright.birthrightCompletionTime + (PrimalBirthright.birthrightCompletionTimeStacking * (RoR2.Util.GetItemCountGlobal(SS2Content.Items.PrimalBirthright.itemIndex, false) - 1));
            }
            
            if (PrimalBirthrightObjectiveToken.instanceList.Count == 0)
            {
                completed = true;
                return;
            }
            
            PrimalBirthright.Behavior.timerComponent = this;
            ObjectivePanelController.collectObjectiveSources += PrimalBirthright.OnCollectObjectiveSources;
        }

        private void OnDisable()
        {
            ObjectivePanelController.collectObjectiveSources -= PrimalBirthright.OnCollectObjectiveSources;
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;

            if (completed)
            {
                return;
            }
            
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
                completed = true;
            }
        }
    }
}