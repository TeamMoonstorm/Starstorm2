using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using MSU;
using RoR2.ContentManagement;
using System.Collections;
using SS2.Modules;
using System.Linq;
using RoR2.Skills;

namespace SS2.Interactables
{
    public sealed class ShockDroneDamaged : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest =>  SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acShockDrone", SS2Bundle.Interactables);

        private SummonMasterBehavior smb;
        private CharacterMaster cm;
        private GameObject bodyPrefab;
        private AkEvent[] droneAkEvents;

        public override void Initialize()
        {
            //This should stop hooking and really just be a global hook that checks for our drones tbh.
            On.EntityStates.Drone.DeathState.OnImpactServer += SpawnShockCorpse;

            //add sound events, the bad way
            smb = InteractablePrefab.GetComponent<SummonMasterBehavior>();
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

            var/*stinky var*/ skillFamily = SS2Assets.LoadAsset<SkillFamily>("sfShockDroneCommand", SS2Bundle.Interactables);
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
            if (AssetCollection.FindAsset<DroneDef>("ddShockDrone") == null)
            {
                DroneDef droneDef = SS2Assets.LoadAsset<DroneDef>("ddShockDrone", SS2Bundle.Interactables);
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

        private void SpawnShockCorpse(On.EntityStates.Drone.DeathState.orig_OnImpactServer orig, EntityStates.Drone.DeathState self, Vector3 contactPoint)
        {
            if (self.characterBody.bodyIndex == BodyCatalog.FindBodyIndexCaseInsensitive("ShockDroneBody"))
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
