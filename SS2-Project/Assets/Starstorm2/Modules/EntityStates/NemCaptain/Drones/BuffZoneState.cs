using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using UnityEngine.Networking;

namespace EntityStates.NemCaptainDrone
{
    public class BuffZoneState : BaseNemCaptainDroneState
    {
        public static GameObject buffZonePrefab;
        public static float lifetime;
        private GameObject buffZoneInstance;

        protected override Interactability GetInteractability(Interactor activator)
        {
            return Interactability.Disabled;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                buffZoneInstance = UnityEngine.Object.Instantiate(buffZonePrefab, transform.position, transform.rotation);
                buffZoneInstance.GetComponent<TeamFilter>().teamIndex = teamFilter.teamIndex;
                NetworkServer.Spawn(buffZoneInstance);
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
            if (buffZoneInstance)
            {
                Destroy(buffZoneInstance);
            }
            base.OnExit();
        }
    }
}
