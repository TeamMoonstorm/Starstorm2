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
            if (!NetworkServer.active) return;
            timer = PrimalBirthright.birthrightCompletionTime + (PrimalBirthright.birthrightCompletionTimeStacking * (RoR2.Util.GetItemCountGlobal(SS2Content.Items.PrimalBirthright.itemIndex, false) - 1));
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
            
            spawnedMeteors = true;
            gameObject.GetComponent<BirthrightMeteorStormController>().enabled = true;
            gameObject.GetComponent<ChildLocator>().FindChild("PPin").gameObject.SetActive(true);
            SS2Log.Debug("spawning meteors !!");
        }
    }
}