using RoR2;
using UnityEngine;
namespace Moonstorm.Starstorm2.Components
{
    public class AnimateWeatherControllerToInitial : MonoBehaviour
    {
        public float timeToLerp;

        private float LerpWhenStarted;
        private float stopwatch;
        private float expectedLerp;
        private bool isDestroying;
        private SceneWeatherController.WeatherParams cachedParams;

        void Awake()
        {
            if (!SceneWeatherController.instance || SceneWeatherController.instance.weatherLerp == 0)
            {
                Destroy(this);
                return;
            }
            LerpWhenStarted = SceneWeatherController.instance.weatherLerp;
            expectedLerp = LerpWhenStarted;
        }

        void Update()
        {
            stopwatch += Time.deltaTime;

            if (!SceneWeatherController.instance)
            {
                Destroy(this);
                isDestroying = true;
                return;
            }
            if (expectedLerp != SceneWeatherController.instance.weatherLerp)
            {
                expectedLerp = -1;
                return;
            }

            float t = stopwatch / timeToLerp;
            expectedLerp = Mathf.Lerp(LerpWhenStarted, 0, stopwatch / timeToLerp);
            SceneWeatherController.instance.weatherLerp = expectedLerp;
            cachedParams = SceneWeatherController.instance.GetWeatherParams(expectedLerp);

            if (timeToLerp - stopwatch <= 0f)
            {
                Destroy(this);
                isDestroying = true;
            }
        }
        private void LateUpdate()
        {
            if (isDestroying || expectedLerp == SceneWeatherController.instance.weatherLerp)
                return;
            SceneWeatherController.instance.initialWeatherParams = cachedParams;
            SceneWeatherController.instance.Update();
            Destroy(this);
        }
    }
}