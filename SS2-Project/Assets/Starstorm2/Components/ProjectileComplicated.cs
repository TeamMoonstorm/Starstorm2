using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2
{
    // Handles movement like ProjectileSimple does, but with randomizable and animatable values.
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileComplicated : MonoBehaviour
    {
        [Serializable]
        public struct ProjectileStateData
        {
            public string name;

            public float minDuration;
            public float maxDuration;

            public float minForwardSpeed;
            public float maxForwardSpeed;
            public AnimationCurve forwardSpeedCurve;

            public float minOscillateMagnitude;
            public float maxOscillateMagnitude;
            public AnimationCurve oscillateMagnitudeCurve;

            public float minOscillateSpeed;
            public float maxOscillateSpeed;
            public AnimationCurve oscillateSpeedCurve;

            public float minRotationTowardsTargetSpeed;
            public float maxRotationTowardsTargetSpeed;
            public AnimationCurve rotationTowardsTargetSpeedCurve;
        }
        public struct ProjectileState
        {
            public ProjectileStateData data;

            public float startTime;
            public float endTime;

            public float forwardSpeed;
            public float oscillateMagnitude;
            public float oscillateSpeed;
            public float rotationTowardsTargetSpeed;
        }

        public ProjectileStateData[] keyframes = Array.Empty<ProjectileStateData>();

        public bool randomizeOscillationDirection = true;
        private bool reverseYaw;
        private bool reversePitch;

        private ProjectileState[] projectileStates;

        private float stopwatch;

        private ProjectileController controller;
        private ProjectileTargetComponent targetComponent;
        private Rigidbody rigidbody;
        private void Awake()
        {
            controller = GetComponent<ProjectileController>();
            targetComponent = GetComponent<ProjectileTargetComponent>();
            rigidbody = GetComponent<Rigidbody>();

            // i guess i dont know how projectiles work
            if (true)//NetworkServer.active)
            {
                projectileStates = new ProjectileState[keyframes.Length];
                float startTime = 0;
                for (int i = 0; i < keyframes.Length; i++)
                {
                    var state = new ProjectileState();

                    state.data = keyframes[i];

                    state.startTime = startTime;
                    float duration = UnityEngine.Random.Range(state.data.minDuration, state.data.maxDuration);
                    state.endTime = startTime + duration;

                    state.forwardSpeed = UnityEngine.Random.Range(state.data.minForwardSpeed, state.data.maxForwardSpeed);
                    state.oscillateMagnitude = UnityEngine.Random.Range(state.data.minOscillateMagnitude, state.data.maxOscillateMagnitude);
                    state.oscillateSpeed = UnityEngine.Random.Range(state.data.minOscillateSpeed, state.data.maxOscillateSpeed);
                    state.rotationTowardsTargetSpeed = UnityEngine.Random.Range(state.data.minRotationTowardsTargetSpeed, state.data.maxRotationTowardsTargetSpeed);

                    projectileStates[i] = state;

                    startTime = state.endTime;
                }

                if (randomizeOscillationDirection)
                {
                    reverseYaw = UnityEngine.Random.Range(0f, 1f) > 0.5f;
                    reversePitch = UnityEngine.Random.Range(0f, 1f) > 0.5f;
                }
            }
        }

        private void FixedUpdate()
        {
            if (true)//controller.hasAuthority)
            {
                FixedUpdateAuthority();
            }
        }

        private void FixedUpdateAuthority()
        {
            stopwatch += Time.fixedDeltaTime;
            ProjectileState currentState = GetCurrentState(stopwatch);

            float stateDuration = currentState.endTime - currentState.startTime;
            float t = (stopwatch - currentState.startTime) / stateDuration;

            float forwardSpeed = currentState.forwardSpeed * (currentState.data.forwardSpeedCurve?.Evaluate(t) ?? 1);
            float oscillateMagnitude = currentState.oscillateMagnitude * (currentState.data.oscillateMagnitudeCurve?.Evaluate(t) ?? 1);
            float oscillateSpeed = currentState.oscillateSpeed * (currentState.data.oscillateSpeedCurve?.Evaluate(t) ?? 1);
            float rotationTowardsTargetSpeed = currentState.rotationTowardsTargetSpeed * (currentState.data.rotationTowardsTargetSpeedCurve?.Evaluate(t) ?? 1);

            float deltaHeight = Mathf.Sin(stopwatch * oscillateSpeed);

            if (rotationTowardsTargetSpeed != 0f && targetComponent && targetComponent.target)
            {
                Vector3 between = targetComponent.target.transform.position - transform.position;
                if (between != Vector3.zero)
                {
                    transform.forward = Vector3.RotateTowards(transform.forward, between, rotationTowardsTargetSpeed * Mathf.Deg2Rad, 0f);
                }
            }
            if (rigidbody)
            {
                Vector3 right = (transform.right * deltaHeight * oscillateMagnitude);
                if (reverseYaw) right *= -1f;

                Vector3 up = (transform.up * deltaHeight * oscillateMagnitude);
                if (reversePitch) up *= -1f;

                rigidbody.velocity = (transform.forward * forwardSpeed) + right + up;
            }

        }

        private ProjectileState GetCurrentState(float currentTime)
        {
            if (projectileStates.Length == 0)
            {
                SS2Log.Error($"ProjectileComplicated.projectileStates is empty for {gameObject.name}");
                Destroy(gameObject);
                return default(ProjectileState);
            }
            for (int i = 0; i < projectileStates.Length; i++)
            {
                var state = projectileStates[i];
                if (currentTime >= state.startTime && currentTime < state.endTime)
                {
                    return state;
                }
            }
            return projectileStates[projectileStates.Length - 1];
        }
    }
}
