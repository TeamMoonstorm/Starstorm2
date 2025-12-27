using UnityEngine;
using RoR2;
using System;
namespace SS2.Components
{
    [RequireComponent(typeof(EffectComponent))]
    public class EffectWorldRotationFromStartVector : MonoBehaviour
    {
        public Transform[] transformsToRotate = Array.Empty<Transform>();

        private void Start()
        {
            EffectComponent effectComponent = base.GetComponent<EffectComponent>();
            var start = effectComponent.effectData.start;
            var rotation = Util.QuaternionSafeLookRotation(start);
            for (int i = 0; i < transformsToRotate.Length; i++)
            {
                var transform = transformsToRotate[i];
                if (transform)
                {
                    transform.rotation = rotation;
                }
            }
        }
    }
}
