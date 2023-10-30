using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.Components
{
    public class ShockDroneVFX : NetworkBehaviour
    {
        private ModelLocator modelLocator;
        private GameObject model;
        private ChildLocator childLocator;
        private GameObject chargeVFX;
        private SkillLocator skillLocator;
        private HealthComponent healthComponent;
        private bool chargeEnabled;
        private AkEvent[] droneAkEvents;

        private void Awake()
        {
            modelLocator = GetComponent<ModelLocator>();
            model = modelLocator.modelTransform.gameObject;
            childLocator = model.GetComponent<ChildLocator>();
            chargeVFX = childLocator.FindChild("ChargeSparks").gameObject;
            skillLocator = GetComponent<SkillLocator>();
            healthComponent = GetComponent<HealthComponent>();

            var droneBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion();

            droneAkEvents = droneBody.GetComponents<AkEvent>();

            foreach (AkEvent akEvent in droneAkEvents)
            {
                var akEventType = akEvent.GetType();
                var newComponent = gameObject.AddComponent(akEventType);

                var fields = akEventType.GetFields();

                foreach (var field in fields)
                {
                    var value = field.GetValue(akEvent);
                    field.SetValue(newComponent, value);
                }
            }
        }

        private void FixedUpdate()
        {
            if (skillLocator.primary.stock == 0 || !healthComponent.alive)
            {
                chargeVFX.SetActive(false);
            }
            else
            {
                chargeVFX.SetActive(true);
            }
        }
    }
}
