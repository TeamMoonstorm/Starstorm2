﻿using UnityEngine;
namespace SS2.Components
{
    public class LerpParticleSystemEmission : MonoBehaviour
    {
        public ParticleSystem particleSystem;
        public float duration = 3;
        public AnimationCurve lerp;

        private ParticleSystem.EmissionModule emission;
        private float stopwatch;
        private float initialEmission;
        void Start()
        {
            emission = particleSystem.emission;
            initialEmission = emission.rateOverTimeMultiplier;           
        }

        private void OnEnable()
        {
            if (!particleSystem.isPlaying) //
                particleSystem.Play();
        }

        public void FixedUpdate()
        {
            emission.rateOverTimeMultiplier = initialEmission * lerp.Evaluate(stopwatch / duration);
            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= initialEmission)
                enabled = false;
        }
    }
}