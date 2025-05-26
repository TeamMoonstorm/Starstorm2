﻿using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using MSU;
using RoR2.ContentManagement;
using System.Collections;
using SS2.Modules;
using System.Linq;
using System;
using RoR2.EntitlementManagement;

namespace SS2.Interactables
{
    public sealed class Mimic : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest =>  SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acMimicInteractable", SS2Bundle.Monsters);

        private SummonMasterBehavior smb;
        private CharacterMaster cm;
        private GameObject bodyPrefab;
        private AkEvent[] droneAkEvents;

        public override void Initialize()
        {
            //On.EntityStates.Drone.DeathState.OnImpactServer += SpawnShockCorpse;

            //add sound events, the bad way
            //smb = InteractablePrefab.GetComponent<SummonMasterBehavior>();
            //cm = smb.masterPrefab.GetComponent<CharacterMaster>();
            //bodyPrefab = cm.bodyPrefab;
            //
            //var droneBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion();
            //
            //droneAkEvents = droneBody.GetComponents<AkEvent>();
            //
            //foreach (AkEvent akEvent in droneAkEvents)
            //{
            //    var akEventType = akEvent.GetType();
            //    var newComponent = bodyPrefab.AddComponent(akEventType);
            //
            //    var fields = akEventType.GetFields();
            //
            //    foreach (var field in fields)
            //    {
            //        var value = field.GetValue(akEvent);
            //        field.SetValue(newComponent, value);
            //    }
            //}
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        //private void SpawnShockCorpse(On.EntityStates.Drone.DeathState.orig_OnImpactServer orig, EntityStates.Drone.DeathState self, Vector3 contactPoint)
        //{
        //    if (self.characterBody.bodyIndex == BodyCatalog.FindBodyIndexCaseInsensitive("ShockDroneBody"))
        //    {
        //        DirectorPlacementRule placementRule = new DirectorPlacementRule
        //        {
        //            placementMode = DirectorPlacementRule.PlacementMode.Direct,
        //            position = contactPoint
        //        };
        //        GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(CardProvider.BuildSpawnCardSet().FirstOrDefault(), placementRule, new Xoroshiro128Plus(0UL)));
        //        if (gameObject)
        //        {
        //            PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
        //            if (component && component.costType == CostTypeIndex.Money)
        //            {
        //                component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
        //            }
        //        }
        //
        //    }
        //    else
        //    {
        //        orig(self, contactPoint);
        //    }
        //}
    }
}
