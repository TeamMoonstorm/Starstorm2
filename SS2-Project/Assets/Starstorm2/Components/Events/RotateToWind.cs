using RoR2;
using UnityEngine;
using Moonstorm.Components;

namespace Moonstorm.Starstorm2.Components
{
    public class RotateToWind : MonoBehaviour
    {
        [Tooltip("Whether the tool keeps tracking to the wind vector")]
        public bool trackAfterStart = true;
        [Tooltip("Rotation speed in degrees of the object after initialization")]
        public float rotationSpeed = 30f;

        private float maxRotation
        {
            get
            {
                return Time.deltaTime * rotationSpeed;
            }
        }

        private void Start()
        {
            if (!WindZoneController.instance)
            {
                Destroy(this);
                return;
            }
            //Short-hand for setting to only x and y.
            transform.rotation = Util.QuaternionSafeLookRotation(WindZoneController.instance.windZone.transform.forward, Vector3.up);
            if (!trackAfterStart)
                Destroy(this);
        }

        private void FixedUpdate()
        {
            float angle = Vector2.SignedAngle(transform.eulerAngles, WindZoneController.instance.windZone.transform.eulerAngles);
            if (Mathf.Approximately(0f, angle))
            {
                var degreesMoved = Mathf.MoveTowards(0, angle, maxRotation);
                transform.eulerAngles = Util.RotateVector2(transform.eulerAngles, degreesMoved);
            }
        }
    }
}