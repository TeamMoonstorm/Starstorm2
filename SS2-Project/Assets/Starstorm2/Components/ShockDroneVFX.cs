using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
namespace SS2.Components
{
    public class ShockDroneVFX : NetworkBehaviour
    {
        private GameObject chargeVFX;
        private SkillLocator skillLocator;
        private HealthComponent healthComponent;
        private AkEvent[] droneAkEvents;

        private void Awake()
        {
            if (!TryGetComponent<ModelLocator>(out var modelLocator))
            {
                SS2Log.Error("ShockDroneVFX: missing ModelLocator");
                return;
            }

            var modelTransform = modelLocator.modelTransform;
            if (!modelTransform)
            {
                SS2Log.Error("ShockDroneVFX: modelTransform is null");
                return;
            }

            if (modelTransform.TryGetComponent<ChildLocator>(out var childLocator))
            {
                var chargeSparks = childLocator.FindChild("ChargeSparks");
                if (chargeSparks)
                    chargeVFX = chargeSparks.gameObject;
            }

            TryGetComponent(out skillLocator);
            TryGetComponent(out healthComponent);

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
            if (!chargeVFX)
                return;

            if (!skillLocator || !healthComponent || skillLocator.primary.stock == 0 || !healthComponent.alive)
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
