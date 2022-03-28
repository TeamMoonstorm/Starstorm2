using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class AimNoise : MonoBehaviour
    {
        public CameraRigController cameraRigController;
        public float intensity = 1f;
        //how long the noise takes to loop through the textures, repeating the pattern
        public float loopTime = 10f;
        public float noiseStrength = 3f;

        //How many sample of noise to draw for each axis.
        private static int sampleCount = 256;
        //buffer in here so it doesn't skip over the last one.
        public Vector2[] noiseSamples = new Vector2[sampleCount + 1];
        private PitchYawPair velocity = PitchYawPair.zero;
        private float stopwatch;
        private int seed;

        private bool setToDestroy;
        private float destroyIntensity;
        private float destroyTimer;
        private float destroyDuration;
        private int sampleIndex
        {
            get
            {
                return Mathf.FloorToInt(sampleCount * (stopwatch % loopTime / loopTime));
            }
        }
        private float cameraMinPitch
        {
            get
            {
                return cameraRigController.targetParams.cameraParams.data.minPitch.value;
            }
        }
        private float cameraMaxPitch
        {
            get
            {
                return cameraRigController.targetParams.cameraParams.data.maxPitch.value;
            }
        }

        void Awake()
        {
            /*if (!Run.instance)
            {
                Destroy(this);
                return;
            }
            seed = Run.instance.runRNG.nextInt;*/
            seed = 500000;
            CalculateNoise();
            //RoR2Application.onUpdate += PolluteNoise;
        }

        private void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (setToDestroy)
            {
                destroyTimer += Time.fixedDeltaTime;
                intensity = Mathf.Lerp(0, destroyIntensity, destroyTimer / destroyDuration);
                if (destroyTimer >= destroyDuration)
                    Destroy(this);
            }
        }

        /// <summary>
        /// Essentially this entire thing works as a cycle of noise. If you have seen Perlin noise, it's cloudy and ranges from 1-0 in intensity.
        /// But because we need it seamless, instead of using the square that a texture generates, we use only use a circle inside that texture,
        /// so we don't really even need a texture, just 2 cycles of values for x and y (pitch and yaw) to go through. See this. We follow a line like this pretty much: https://res.cloudinary.com/dn1rmdjs5/image/upload/v1566568756/rv/noiseCircle.jpg
        /// </summary>
        public void PolluteNoise()
        {
            /*
            if (!cameraRigController || PauseManager.isPaused)
                return;
            // This should never return a value above the sampleCount but if it does clamp it.
            Vector2 sample = noiseSamples[Mathf.Clamp(sampleIndex, 0, sampleCount)];

            float pitchMovement = sample.y * Mathf.Abs(cameraMinPitch) * intensity;
            float newPitch = Mathf.Clamp(cameraRigController.pitch + pitchMovement, cameraMinPitch, cameraMaxPitch);

            float yawMovement = sample.x * 20f * intensity;
            float newYaw = cameraRigController.yaw + yawMovement;

            var pitchYaw = new PitchYawPair(cameraRigController.pitch, cameraRigController.yaw);
            var desiredPitchYaw = new PitchYawPair(newPitch, newYaw);

            var newPitchYaw = PitchYawPair.SmoothDamp(pitchYaw, desiredPitchYaw, ref velocity, .3f, 5f * intensity);

            cameraRigController.SetPitchYaw(newPitchYaw);
            

            LogCore.LogM($"Current noise {sample}");
            LogCore.LogM($"desiredPitchYaw {desiredPitchYaw.pitch} {desiredPitchYaw.yaw}");
            LogCore.LogM($"newPitchYaw {newPitchYaw.pitch} {newPitchYaw.yaw}");*/
        }

        public void Update()
        {
            // This should never return a value above the sampleCount but if it does clamp it.
            Vector2 sample = noiseSamples[Mathf.Clamp(sampleIndex, 0, sampleCount)];

            float pitchMovement = Mathf.Lerp(-10f, 10f, sample.y) * intensity;
            float newPitch = Mathf.Clamp(transform.eulerAngles.x + pitchMovement, -70, 70);

            float yawMovement = sample.x * 20f * intensity;
            float newYaw = transform.eulerAngles.y + yawMovement;

            var pitchYaw = new PitchYawPair(transform.eulerAngles.x, transform.eulerAngles.y);
            var desiredPitchYaw = new PitchYawPair(newPitch, newYaw);

            var pair = PitchYawPair.SmoothDamp(pitchYaw, desiredPitchYaw, ref velocity, .3f, 5f * intensity);
            transform.eulerAngles = new Vector3(pair.pitch, pair.yaw, transform.eulerAngles.z);
        }


        private void CalculateNoise()
        {
            for (int i = 0; i < sampleCount; i++)
            {
                //how far we are through the loop.
                float angle = i * 2 * Mathf.PI / sampleCount;
                //Our horizontal/vertical displacements from the center of the loop, which is at the seed
                float x = Mathf.Cos(angle) * noiseStrength;
                float y = Mathf.Sin(angle) * noiseStrength;
                float xNoise = Mathf.PerlinNoise(x + seed, y);
                //The -0.5 * 2f sets each y value to be normalized between -1 and 1 instead of 0 and 1 since it has positive and negative aim angles.
                float yNoise = Mathf.PerlinNoise(y + seed, x);
                noiseSamples[i] = new Vector2(xNoise, yNoise);
            }
        }

        public void SetToDestroy(float duration)
        {
            setToDestroy = true;
            destroyDuration = duration;
            destroyIntensity = intensity;
        }

        public void OnDestroy()
        {
            RoR2Application.onUpdate -= PolluteNoise;
        }
    }
}