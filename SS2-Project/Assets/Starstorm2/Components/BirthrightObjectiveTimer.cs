using System;
using System.Collections.Generic;
using RoR2;
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
        public static BirthrightObjectiveTimer instance;
        
        [SyncVar] 
        public float timer;
        [SyncVar] 
        public bool completed;
        
        public bool spawnedMeteors;

        private void OnEnable()
        {
            instance = this;
            
            if (NetworkServer.active)
            {
                // im not sure how stacking should work here.,,. being generous to players ! each one gets their own initial stacking ! 
                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController._instancesReadOnly)
                {
                    int? itemCount = pcmc.master?.inventory?.GetItemCountEffective(SS2Content.Items.PrimalBirthright.itemIndex);
                    if (itemCount is > 0) // rider wants to turn it into a is so its probably fine .,,
                    {
                        timer += PrimalBirthright.birthrightCompletionTime + (PrimalBirthright.birthrightCompletionTimeStacking * (itemCount.Value - 1));
                    }
                }
            }
            
            //since the object spawns on enable itll spawn on initial pickup and dont want the storm to go off ,.,. 
            if (PrimalBirthrightObjectiveToken.instanceList.Count == 0)
            {
                completed = true;
                return;
            }

            ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
        }

        private void OnDisable()
        {
            ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
        }

        private void OnDestroy()
        {
            instance = null; // turns out static refs like this dont make it null once the object is destroyed .,.,., ,. thank you swuff, .,. .
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
                ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
                completed = true;
            }
        }
        
        public static void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            var newObjective = new ObjectivePanelController.ObjectiveSourceDescriptor
            {
                master = master,
                objectiveType = typeof(PrimalBirthrightObjectiveTracker),
                source = PrimalBirthright.primalToken
            };

            objectiveSourcesList.Add(newObjective);
        }
    }
    
    public class PrimalBirthrightObjectiveTracker : ObjectivePanelController.ObjectiveTracker
    {
        public override string GenerateString()
        {
            if (!BirthrightObjectiveTimer.instance)
            {
                // this *shouldnt* happen but ,.., swagever go my base case, ,. 
                return string.Format(Language.GetString("SS2_BIRTHRIGHT_OBJECTIVE"), PrimalBirthrightObjectiveToken.instanceList.Count, PrimalBirthright.birthrightCompletionTime + (PrimalBirthright.birthrightCompletionTimeStacking * (Util.GetItemCountGlobal(SS2Content.Items.PrimalBirthright.itemIndex, false) - 1)));;
            }
            
            if (BirthrightObjectiveTimer.instance.timer <= 0)
            {
                return PrimalBirthrightObjectiveToken.instanceList.Count == 0 ? Language.GetString("SS2_BIRTHRIGHT_OBJECTIVEFAILEDCLAIMEDALL") : string.Format(Language.GetString("SS2_BIRTHRIGHT_OBJECTIVEFAILED"), PrimalBirthrightObjectiveToken.instanceList.Count);
            }
            
            string text = string.Format(Language.GetString("SS2_BIRTHRIGHT_OBJECTIVE"), PrimalBirthrightObjectiveToken.instanceList.Count, ((int)(BirthrightObjectiveTimer.instance.timer) + 1));
            if (BirthrightObjectiveTimer.instance.timer < 30 && (int)(Time.time * 12f) % 2 == 0)
            {
                text = $"<style=cDeath>{text}</style>";
            }

            return text;
        }

        public override bool IsDirty()
        {
            return true;
        }
    }
}