using System;
using RoR2;
using UnityEngine;

namespace EntityStates.Destructible
{
    public class FindSurface : BaseState
    {
        public static int raycastCount = 12;

        public static float maxRaycastLength = 50;

        public static float raycastSphereYOffset = 0;

        public static float raycastMinimumAngle = 10;

        public static float raycastMaximumAngle = 90;
        public override void OnEnter()
        {
            base.OnEnter();
            Vector3 origin = new Vector3(characterBody.corePosition.x, characterBody.corePosition.y + raycastSphereYOffset, characterBody.corePosition.z);
            if (isAuthority)
            {
                raycastMinimumAngle = Mathf.Clamp(raycastMinimumAngle, 0f, raycastMaximumAngle);
                raycastMaximumAngle = Mathf.Clamp(raycastMaximumAngle, raycastMinimumAngle, 90f);
                raycastCount = Mathf.Abs(raycastCount);
                float spreadAngle = 360f / (float)raycastCount;
                for (int i = 0; i < raycastCount; i++)
                {
                    float minInclusive = spreadAngle * (float)i;
                    float maxInclusive = spreadAngle * (float)(i + 1) - 1f;
                    float f = UnityEngine.Random.Range(minInclusive, maxInclusive) * Mathf.Deg2Rad;
                    float f2 = UnityEngine.Random.Range(raycastMinimumAngle, raycastMaximumAngle) * Mathf.Deg2Rad;
                    float x = Mathf.Cos(f);
                    float y = Mathf.Sin(f2);
                    float z = Mathf.Sin(f);
                    Vector3 direction = new Vector3(x, y, z);
                    if (Physics.Raycast(origin, direction, out RaycastHit raycastHit, maxRaycastLength, LayerIndex.world.mask))
                    {
                        transform.position = raycastHit.point;
                        transform.up = raycastHit.normal;
                        return;
                    }
                }
            }

            outer.SetNextStateToMain();
        }

        
    }
}
