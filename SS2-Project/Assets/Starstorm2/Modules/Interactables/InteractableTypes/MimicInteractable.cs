using RoR2;
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
    public sealed class MimicInteractable : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest =>  SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acMimicInteractable", SS2Bundle.Monsters);

        private SummonMasterBehavior smb;
        private CharacterMaster cm;
        private GameObject bodyPrefab;
        private AkEvent[] droneAkEvents;

        public override void Initialize()
        {

        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
