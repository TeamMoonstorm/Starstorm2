using System;
using UnityEngine;
using UnityEngine.Events;

namespace Moonstorm.Starstorm2.Components
{
    public class EventStateEffect : MonoBehaviour
    {
        [HideInInspector]
        public float intensityMultiplier = 1f;

        private bool isEnding = false;
        private float endingDuration = 0;
        private float endingTimer = 0;


        public void OnEffectStart()
        {
            onEffectStart.Invoke(intensityMultiplier);
        }

        public void OnEndingStart(float endingDuration)
        {
            onEndingStart.Invoke(endingDuration);
            this.endingDuration = endingDuration;
            isEnding = true;
        }

        private void FixedUpdate()
        {
            if (isEnding)
            {
                endingTimer += Time.fixedDeltaTime;
                if (endingTimer >= endingDuration)
                    Destroy(gameObject);
            }
        }


        [Space]
        [Tooltip("Actions to be called when the warning starts")]
        public EventStateEvent onEffectStart = new EventStateEvent();
        public EventStateEvent onEndingStart = new EventStateEvent();
        public EventStateEvent onDestroyEffect = new EventStateEvent();

        [Serializable]
        public class EventStateEvent : UnityEvent<float> { }
    }
}