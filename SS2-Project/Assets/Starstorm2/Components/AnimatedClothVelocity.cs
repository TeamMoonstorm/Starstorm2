using UnityEngine;
using Moonstorm.Components;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(Cloth))]
    public class AnimatedClothVelocity : MonoBehaviour
    {
        public float intensity;
        public bool includeWind;
        public AnimationCurve overallCurve;
        public AnimationCurve xAnimationCurve;
        public AnimationCurve yAnimationCurve;
        public AnimationCurve zAnimationCurve;
        public bool useOverallCurveOnly;

        private Cloth cloth;
        private float stopwatch;

        void Awake()
        {
            cloth = GetComponent<Cloth>();
        }

        void Update()
        {
            float overallEvaluation = overallCurve.Evaluate(stopwatch);
            Vector3 velocity = new Vector3(overallEvaluation, overallEvaluation, overallEvaluation);
            if (!useOverallCurveOnly)
            {
                float x = xAnimationCurve.Evaluate(stopwatch);
                float y = yAnimationCurve.Evaluate(stopwatch);
                float z = zAnimationCurve.Evaluate(stopwatch);
                velocity += new Vector3(x, y, z);
            }
            cloth.externalAcceleration = velocity * intensity;
            if (includeWind && WindZoneController.instance)
            {
                WindZone wind = WindZoneController.instance.windZone;
                float windIntensity = wind.windMain;
                windIntensity += Mathf.MoveTowards(wind.windTurbulence, wind.windTurbulence * Random.Range(0.3f, 1.5f), Random.Range(0.6f, 0.8f) * Time.deltaTime);
                if (stopwatch % wind.windPulseFrequency <= 0.25f) // tbh a random number, may not work all the time.
                    windIntensity += wind.windPulseMagnitude;
                cloth.externalAcceleration += wind.transform.forward * windIntensity;
            }
            stopwatch += Time.deltaTime;
        }
    }
}