using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using UnityEngine.Networking;

namespace EntityStates.NemCaptainDrone
{
    public class BlackHoleState : BaseNemCaptainDroneState
    {
        public static GameObject blackHolePrefab;
        public static float lifetime;
        private GameObject blackHoleInstance;

        protected override Interactability GetInteractability(Interactor activator)
        {
            return Interactability.Disabled;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                blackHoleInstance = UnityEngine.Object.Instantiate(blackHolePrefab, transform.position, transform.rotation);
                blackHoleInstance.GetComponent<TeamFilter>().teamIndex = teamFilter.teamIndex;
                NetworkServer.Spawn(blackHoleInstance);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= lifetime)
            {
                Debug.Log("I should be dead right now!!!");
                outer.SetNextState(new ExitState());
            }
        }

        public override void OnExit()
        {
            if (blackHoleInstance)
            {
                Destroy(blackHoleInstance);
            }
            base.OnExit();
        }
    }
}
