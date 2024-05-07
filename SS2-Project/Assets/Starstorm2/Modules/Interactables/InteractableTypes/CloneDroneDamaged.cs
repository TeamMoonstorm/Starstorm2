using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using MSU;
using RoR2.ContentManagement;
using System.Collections;
using SS2.Modules;
using System.Linq;

namespace SS2.Interactables
{
    public sealed class CloneDroneDamaged : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest()
        {
            return SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acCloneDroneDamaged", SS2Bundle.Interactables);
        }

        private GameObject _interactable;
        private SummonMasterBehavior smb;
        private CharacterMaster cm;
        private GameObject bodyPrefab;
        private AkEvent[] droneAkEvents;

        public override void Initialize()
        {
            //This should stop hooking and really just be a global hook that checks for our drones tbh.
            On.EntityStates.Drone.DeathState.OnImpactServer += SpawnCloneCorpse;

            //add sound events, the bad way
            smb = _interactable.GetComponent<SummonMasterBehavior>();
            cm = smb.masterPrefab.GetComponent<CharacterMaster>();
            bodyPrefab = cm.bodyPrefab;

            var droneBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion();

            droneAkEvents = droneBody.GetComponents<AkEvent>();

            foreach (AkEvent akEvent in droneAkEvents)
            {
                var akEventType = akEvent.GetType();
                var newComponent = bodyPrefab.AddComponent(akEventType);

                var fields = akEventType.GetFields();

                foreach (var field in fields)
                {
                    var value = field.GetValue(akEvent);
                    field.SetValue(newComponent, value);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * GameObject - "CloneDroneBroken" - Interactables
             * InteractableCardProvider - "???" - Interactables
             */
            yield break;
        }

        
        private void SpawnCloneCorpse(On.EntityStates.Drone.DeathState.orig_OnImpactServer orig, EntityStates.Drone.DeathState self, Vector3 contactPoint)
        {
            orig(self, contactPoint);
            if (self.characterBody.bodyIndex == BodyCatalog.FindBodyIndexCaseInsensitive("CloneDroneBody"))
            {
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    position = contactPoint
                };
                GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(CardProvider.BuildSpawnCardSet().FirstOrDefault(), placementRule, new Xoroshiro128Plus(0UL)));
                if (gameObject)
                {
                    PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                    if (component && component.costType == CostTypeIndex.Money)
                    {
                        component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                    }
                }
            }
            else
            {
                orig(self, contactPoint);
            }
        }
    }
}
