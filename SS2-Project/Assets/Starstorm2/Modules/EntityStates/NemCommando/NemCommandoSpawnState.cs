using EntityStates.Generic;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCommando
{
    public class NemCommandoSpawnState : EntityStates.Nemmando.Spawn
    {
        public override void FixedUpdate()
        {
            fixedAge += GetDeltaTime();

            if (isAuthority && fixedAge >= minimumIdleDuration)
            {
                outer.SetNextState(new NemCommandoAppearState());
            }
        }
    }
}