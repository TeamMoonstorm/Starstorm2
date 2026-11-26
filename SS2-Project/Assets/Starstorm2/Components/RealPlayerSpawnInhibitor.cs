using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2.Navigation;
using RoR2;
using UnityEngine;

namespace SS2
{
    // original behavior uses OnEnable, which is too late to stop player spawns
    // will add to the instancesList twice but thats fine
    public class RealPlayerSpawnInhibitor : PlayerSpawnInhibitor
    {
        private void Awake()
        {
            PlayerSpawnInhibitor.instancesList.Add(this);
        }

        private void OnDestroy()
        {
            PlayerSpawnInhibitor.instancesList.Remove(this);
        }
    }
}
