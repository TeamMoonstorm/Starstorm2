using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using UnityEngine.Networking;

namespace EntityStates.NemCaptainDrone
{
    public class HealZoneState : BaseNemCaptainDroneState
    {
        //unfortunately due to the nature of ESCs i can not simply just make this inherit.
        public static GameObject healZonePrefab;
        public static float lifetime;
        private GameObject healZoneInstance;

        protected override Interactability GetInteractability(Interactor activator)
        {
            return Interactability.Disabled;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                healZoneInstance = UnityEngine.Object.Instantiate(healZonePrefab, transform.position, transform.rotation);
                healZoneInstance.GetComponent<TeamFilter>().teamIndex = teamFilter.teamIndex;
                NetworkServer.Spawn(healZoneInstance);
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
            if (healZoneInstance)
            {
                Destroy(healZoneInstance);
            }
            base.OnExit();
        }
    }
}
