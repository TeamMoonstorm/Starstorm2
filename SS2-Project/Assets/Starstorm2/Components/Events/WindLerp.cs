using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class WindLerp : MonoBehaviour
    {
        public WindZone windZone;
        public float windMain;
        public float windTurbulence;
        public float windPulseMagnitude;
        public float windPulseFrequency;
        public float lerpTime;

        private float initialWindMain;
        private float initialWindTurbulence;
        private float initialWindPulseMagnitude;
        private float initialWindPulseFrequency;
        private float stopwatch;

        private void Start()
        {
            initialWindMain = windZone.windMain;
            initialWindTurbulence = windZone.windTurbulence;
            initialWindPulseMagnitude = windZone.windPulseMagnitude;
            initialWindPulseFrequency = windZone.windPulseFrequency;
        }

        void FixedUpdate()
        {
            windZone.windMain = Mathf.Lerp(initialWindMain, windMain, stopwatch / lerpTime);
            windZone.windTurbulence = Mathf.Lerp(initialWindTurbulence, windTurbulence, stopwatch / lerpTime);
            windZone.windPulseMagnitude = Mathf.Lerp(initialWindPulseMagnitude, windPulseMagnitude, stopwatch / lerpTime);
            windZone.windPulseFrequency = Mathf.Lerp(initialWindPulseFrequency, windPulseFrequency, stopwatch / lerpTime);

            if (stopwatch >= lerpTime)
                enabled = false;
            stopwatch += Time.fixedDeltaTime;
        }
    }
}