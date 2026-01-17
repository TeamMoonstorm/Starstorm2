using System.Collections.Generic;
using UnityEngine;
using RoR2;
using SS2.Equipments;
namespace SS2.Components
{
    public class PickupMagnet : MonoBehaviour
    {
        // this thing fucking AWESOME. im so GENIUS
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.GravitatePickup.ctor += GravitatePickup_ctor;
            RoR2Application.onFixedUpdate += CleanList;
        }

        private static void GravitatePickup_ctor(On.RoR2.GravitatePickup.orig_ctor orig, GravitatePickup self)
        {
            orig(self);
            trackedPickups.Add(self);
        }

        public static List<GravitatePickup> trackedPickups = new List<GravitatePickup>();

        private static void CleanList()
        {
            for (int i = trackedPickups.Count - 1; i >= 0; i--)
            {
                GravitatePickup pickup = trackedPickups[i];
                if (!pickup)
                {
                    trackedPickups.RemoveAt(i);
                }
            }
        }

        private float stopwatch;
        private struct Pickup
        {
            public GravitatePickup gravComponent;
            public Rigidbody rigidbody;
            public Vector3 target;
            public int layer;
        }
        private List<Pickup> pickupsToPull = new List<Pickup>();

        private void Start()
        {
            StartPull();
        }

        private void StartPull()
        {
            List<GenericPickupController> pickups = InstanceTracker.GetInstancesList<GenericPickupController>();
            float spreadAngle = 360f / pickups.Count;
            for (int i = 0; i < pickups.Count; i++)
            {
                float angle = i * spreadAngle;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * base.transform.forward;
                bool hit = Physics.SphereCast(base.transform.position, 1f, direction, out RaycastHit raycastHit, Magnet.destinationRadius - 1, LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal);
                Vector3 target = base.transform.position + (direction * Magnet.destinationRadius);
                if (hit)
                {
                    target = raycastHit.point;
                }
                GenericPickupController pickup = pickups[i];
                pickupsToPull.Add(new Pickup { rigidbody = pickup.GetComponent<Rigidbody>(), target = target, layer = LayerIndex.fakeActor.intVal });
                pickup.gameObject.layer = LayerIndex.noCollision.intVal;
            }
            foreach (GravitatePickup pickup in trackedPickups)
            {
                pickup.gravitateTarget = null;
                pickupsToPull.Add(new Pickup { gravComponent = pickup, rigidbody = pickup.rigidbody, target = base.transform.position, layer = pickup.gameObject.layer });
            }
        }

        private void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= Magnet.pullDuration)
            {
                Destroy(base.gameObject);
                return;
            }
            for(int i = 0; i < pickupsToPull.Count; i++)
            {
                Pickup pickup = pickupsToPull[i];
                if (pickup.gravComponent && pickup.gravComponent.gravitateTarget) continue; // let coffee beans and stuff gravitate to player
                if(pickup.rigidbody)
                {
                    pickup.rigidbody.velocity = Vector3.zero;
                    //using transforms is icky but im too lazy to fuck with layers
                    pickup.rigidbody.transform.position = Vector3.MoveTowards(pickup.rigidbody.transform.position, pickup.target, Magnet.pullSpeed * Time.fixedDeltaTime);
                }
                
            }
        }

        private void OnDestroy()
        {
            foreach (Pickup pickup in pickupsToPull)
            {
                if (pickup.rigidbody)
                {
                    pickup.rigidbody.gameObject.layer = pickup.layer;
                }
            }
        }
    }
}

