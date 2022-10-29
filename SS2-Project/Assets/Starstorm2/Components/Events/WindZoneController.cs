using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class WindZoneController : MonoBehaviour
    {
        public static WindZoneController instance
        {
            get
            {
                return _instance;
            }
        }
        private static WindZoneController _instance;

        private void OnEnable()
        {
            if (!_instance)
                _instance = this;
        }
        private void OnDisable()
        {
            if (_instance == this)
                _instance = null;
        }

        public WindZone windZone;

        public WindParams initialWindParams;

        public WindParams finalWindParams;

        [Range(0f, 1f)]
        public float windLerp;

        [Serializable]
        public struct WindParams
        {
            public float windMain;
            public float windTurbulence;
            public float windPulseMagnitude;
            public float windPulseFrequency;
        }

        public WindParams GetWindParams(float t)
        {
            return new WindParams
            {
                windMain = Mathf.Lerp(initialWindParams.windMain, finalWindParams.windMain, t),
                windTurbulence = Mathf.Lerp(initialWindParams.windTurbulence, finalWindParams.windTurbulence, t),
                windPulseMagnitude = Mathf.Lerp(initialWindParams.windPulseMagnitude, finalWindParams.windPulseMagnitude, t),
                windPulseFrequency = Mathf.Lerp(initialWindParams.windPulseFrequency, finalWindParams.windPulseFrequency, t)
            };
        }

        void Start()
        {
            if (!windZone)
            {
                var winds = FindObjectsOfType<WindZone>();
                foreach (var wind in winds)
                {
                    if (wind.mode.HasFlag(WindZoneMode.Directional))
                    {
                        windZone = wind;
                        break;
                    }
                }
                if (!windZone)
                {
                    Destroy(this);
                    return;
                }
            }
            initialWindParams = new WindParams
            {
                windMain = windZone.windMain,
                windTurbulence = windZone.windTurbulence,
                windPulseMagnitude = windZone.windPulseMagnitude,
                windPulseFrequency = windZone.windPulseFrequency
            };
        }

        void Update()
        {
            var windParams = GetWindParams(windLerp);
            windZone.windMain = windParams.windMain;
            windZone.windTurbulence = windParams.windTurbulence;
            windZone.windPulseMagnitude = windParams.windPulseMagnitude;
            windZone.windPulseFrequency = windParams.windPulseFrequency;
        }
    }
}