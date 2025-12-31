using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using MSU;
using RoR2.ContentManagement;
using System.Collections;
using SS2.Modules;
using System.Linq;
using R2API;
namespace SS2.Interactables
{
    public sealed class CloneDroneDamaged : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acCloneDrone", SS2Bundle.Interactables);

        public static GameObject clonedPickupPrefab;

        private SummonMasterBehavior smb;
        private CharacterMaster cm;
        private GameObject bodyPrefab;
        private AkEvent[] droneAkEvents;

        public override void Initialize()
        {
            //This should stop hooking and really just be a global hook that checks for our drones tbh.
            On.EntityStates.Drone.DeathState.OnImpactServer += SpawnCloneCorpse;

            //add sound events, the bad way
            smb = InteractablePrefab.GetComponent<SummonMasterBehavior>();
            cm = smb.masterPrefab.GetComponent<CharacterMaster>();
            bodyPrefab = cm.bodyPrefab;

            clonedPickupPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/GenericPickup.prefab").WaitForCompletion().InstantiateClone("ClonedPickup", false);
            clonedPickupPrefab.AddComponent<ClonedPickup>();

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

            //Temporary assignment of the Generic Command (Equipment Drone's Ram) until a proper Command is made
            var/*stinky var*/ skillFamily = SS2Assets.LoadAsset<SkillFamily>("sfCloneDroneCommand", SS2Bundle.Interactables);
            skillFamily.variants = new SkillFamily.Variant[1];
            var/*stinky var*/ skillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC3/Drone Tech/CommandGeneric.asset").WaitForCompletion();
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.networkedObjectPrefabs.AddSingle(clonedPickupPrefab);

            //Future proof in case this code is forgotten and not removed when the DroneDef is added to the AssetCollection after MSU supports it
            if (AssetCollection.FindAsset<DroneDef>("ddCloneDrone") == null)
            {
                DroneDef droneDef = SS2Assets.LoadAsset<DroneDef>("ddCloneDrone", SS2Bundle.Interactables);
                if (droneDef)
                {
                    contentPack.droneDefs.AddSingle(droneDef);
                }
            }
            base.ModifyContentPack(contentPack);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }


        // if a pickup doesnt have this component, it can be cloned and have this component added with one cloner. the cloned pickup will also have this component but with no cloners
        // if a pickup does have this component but no cloners, it means the pickup was the result of a clone and therefore cannot be cloned
        // if a pickup does have this component but with a cloner, it means the pickup was NOT the result of a clone and therefore CAN be cloned
        public class ClonedPickup : MonoBehaviour 
        { 
            private List<GameObject> cloners = new List<GameObject>();
            public bool CanBeCloned(GameObject cloner) => cloners.Count >= 1 && !cloners.Contains(cloner);
            public bool OnCloned(GameObject cloner)
            {
                if(!cloners.Contains(cloner))
                {
                    cloners.Add(cloner);
                    return true;
                }
                return false;
            }
        }

        private void SpawnCloneCorpse(On.EntityStates.Drone.DeathState.orig_OnImpactServer orig, EntityStates.Drone.DeathState self, Vector3 contactPoint)
        {
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
