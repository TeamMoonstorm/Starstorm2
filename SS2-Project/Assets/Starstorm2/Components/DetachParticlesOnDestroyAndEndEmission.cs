﻿using UnityEngine;
namespace SS2
{
    ///<summary>This is a modified version of RoR2's component so that it can take multiple particle systems</summary>
    public class DetachParticlesOnDestroyAndEndEmission : MonoBehaviour
    {
        private void OnDisable()
        {
            foreach (var particleSystem in particleSystems)
            {
                if(particleSystem)
                {
                    var main = particleSystem.main;
                    var emission = particleSystem.emission;

                    main.stopAction = ParticleSystemStopAction.Destroy;
                    emission.enabled = false;
                    particleSystem.Stop();
                    particleSystem.transform.SetParent(null);
                }             
            }
        }

        public ParticleSystem[] particleSystems;
    }
}
