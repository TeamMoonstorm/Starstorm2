using EntityStates;
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
    public sealed class ShockDroneDamaged : InteractableBase
    {
        public override GameObject Interactable { get; } = SS2Assets.LoadAsset<GameObject>("ShockDroneBroken", SS2Bundle.Interactables);
        private GameObject interactable;
        private SummonMasterBehavior smb;
        private CharacterMaster cm;
        private GameObject bodyPrefab;
        private AkEvent[] droneAkEvents;

        public override MSInteractableDirectorCard InteractableDirectorCard { get; } = SS2Assets.LoadAsset<MSInteractableDirectorCard>("msidcShockDrone", SS2Bundle.Interactables);

        public override void Initialize()
        {
            base.Initialize();

            //add sound events, the bad way
            interactable = InteractableDirectorCard.prefab;
            smb = interactable.GetComponent<SummonMasterBehavior>();
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
    }
}
