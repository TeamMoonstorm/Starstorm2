﻿using EntityStates;
using Moonstorm.Starstorm2.Components;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Interactables
{
    //[DisabledContent]
    //public sealed class CloneDroneDamaged : InteractableBase
    //{
    //    public override GameObject Interactable { get; } = SS2Assets.LoadAsset<GameObject>("CloneDroneBroken", SS2Bundle.Indev);
    //    private GameObject interactable;
    //    private SummonMasterBehavior smb;
    //    private CharacterMaster cm;
    //    private GameObject bodyPrefab;
    //    private AkEvent[] droneAkEvents;

    //    public override MSInteractableDirectorCard InteractableDirectorCard { get; } = SS2Assets.LoadAsset<MSInteractableDirectorCard>("msidcCloneDrone", SS2Bundle.Indev);

    //    public override void Initialize()
    //    {
    //        base.Initialize();

    //        //add sound events
    //        interactable = InteractableDirectorCard.prefab;
    //        smb = interactable.GetComponent<SummonMasterBehavior>();
    //        cm = smb.masterPrefab.GetComponent<CharacterMaster>();
    //        bodyPrefab = cm.bodyPrefab;

    //        var droneBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion();

    //        droneAkEvents = droneBody.GetComponents<AkEvent>();

    //        foreach (AkEvent akEvent in droneAkEvents)
    //        {
    //            var akEventType = akEvent.GetType();
    //            var newComponent = bodyPrefab.AddComponent(akEventType);

    //            var fields = akEventType.GetFields();

    //            foreach (var field in fields)
    //            {
    //                var value = field.GetValue(akEvent);
    //                field.SetValue(newComponent, value);
    //            }
    //        }
    //    }
    //}
}
