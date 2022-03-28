using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    public class NemmandoUnlockComponent : MonoBehaviour
    {
        private HealthComponent healthComponent;

        public static event Action<Run> OnDeath = delegate { };

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
        }

        private void FixedUpdate()
        {
            if (healthComponent && !healthComponent.alive)
            {
                if (healthComponent.body.teamComponent.teamIndex != TeamIndex.Player)
                {
                    OnDeath?.Invoke(Run.instance);
                }
            }
        }
    }
}