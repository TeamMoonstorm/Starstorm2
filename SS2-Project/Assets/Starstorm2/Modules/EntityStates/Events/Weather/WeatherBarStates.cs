using RoR2;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using SS2.UI;
using SS2;

namespace EntityStates.Events
{
    // replace the default scrolling and noise of WeatherBarController with fancy animated values. This runs on the StormController object, not the weather bar itself.
    public class AnimateWeatherBar : EntityState
    {
        public static AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private static float maxMovementDuration = 5f;
        private static float minMovementDuration = 3f;
        private float movementDuration;

        private static float maxSlowdownDuration = 2f;
        private static float minSlowdownDuration = 1f;
        private float slowdownDuration;

        private static float maxFractionStart = 0.45f;
        private static float minFractionStart = 0.25f;
        private float fractionStart;

        private static float maxFractionEnd = 0.7f;
        private static float minFractionEnd = 0.6f;
        private float fractionEnd;

        private enum SubState
        {
            SlowDown,
            MoveFast,
            Idle,
        }
        private SubState subState;
        private float stopwatch;
        private float scrollStart;

        public float targetDuration = -1f;
        private float timeMultiplier;

        // serialize the randomized values
        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(movementDuration);
            writer.Write(slowdownDuration);
            writer.Write(fractionStart);
            writer.Write(fractionEnd);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            movementDuration = reader.ReadSingle();
            slowdownDuration = reader.ReadSingle();
            fractionStart = reader.ReadSingle();
            fractionEnd = reader.ReadSingle();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            WeatherBarController.calcNoise += CalculateNoise;
            WeatherBarController.calcScroll += CalculateScroll;

            if (isAuthority)
            {
                movementDuration = UnityEngine.Random.Range(minMovementDuration, maxMovementDuration);
                slowdownDuration = UnityEngine.Random.Range(minSlowdownDuration, maxSlowdownDuration);
                fractionStart = UnityEngine.Random.Range(minFractionStart, maxFractionStart);
                fractionEnd = UnityEngine.Random.Range(minFractionEnd, maxFractionEnd);
            }

            if (targetDuration > 0)
            {
                float baseDuration = slowdownDuration + movementDuration;
                timeMultiplier = targetDuration / baseDuration;
            }
        }
        public override void OnExit()
        {
            base.OnExit();

            WeatherBarController.calcNoise -= CalculateNoise;
            WeatherBarController.calcScroll -= CalculateScroll;
        }

        private void CalculateScroll(ref float scroll)
        {
            if (subState == SubState.SlowDown)
            {
                // cache the current scroll value so we know where to start during MoveFast
                scrollStart = scroll;
            }
            else if (subState == SubState.MoveFast)
            {
                // Accelerate the scroll bar to the starting point of the next storm level
                float progress = Mathf.Clamp01(stopwatch / movementDuration);
                float curvedProgress = movementCurve.Evaluate(progress);
                float scrollEnd = StormController.instance.currentLevel + fractionStart;
                scroll = Mathf.Lerp(scrollStart, scrollEnd, curvedProgress);
            }
            else if (subState == SubState.Idle)
            {
                // Let the scroll bar progress, but remap it
                float remapScrollFraction = Util.Remap(StormController.instance.levelFractionComplete, 0f, 1f, fractionStart, fractionEnd);
                scroll = StormController.instance.currentLevel + remapScrollFraction;
            }
        }

        private void CalculateNoise(ref WeatherBarController.NoiseValues noise)
        {
            if (subState == SubState.SlowDown)
            {
                // lerp noise down to zero in preparation for MoveFast
                float t = (slowdownDuration - stopwatch) / slowdownDuration;
                t = Mathf.Clamp01(t);
                noise.frequency *= t;
            }
            else if (subState == SubState.MoveFast)
            {
                // no noise during MoveFast
                noise.frequency = 0f;
            }
            else if (subState == SubState.Idle)
            {
                // TODO: change noise strength and frequency based on wind and storm level
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            switch (subState)
            {
                case SubState.SlowDown: SlowDownFixedUpdate(); break;
                case SubState.MoveFast: MoveFastFixedUpdate(); break;
                case SubState.Idle: IdleFixedUpdate(); break;
            }
        }

        private void SlowDownFixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime * timeMultiplier;
            if (stopwatch >= slowdownDuration)
            {
                subState = SubState.MoveFast;
                stopwatch = 0f;
            }
        }
        private void MoveFastFixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime * timeMultiplier;
            if (stopwatch >= movementDuration)
            {
                subState = SubState.Idle;
                stopwatch = 0f;
            }
        }
        private void IdleFixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime * timeMultiplier;
        }
    }
}
