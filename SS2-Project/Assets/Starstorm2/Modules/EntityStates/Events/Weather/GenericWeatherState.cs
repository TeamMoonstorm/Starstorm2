using SS2.Components;
using RoR2;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using SS2;
namespace EntityStates.Events
{
    public abstract class GenericWeatherState : EntityStates.GameplayEvents.GameplayEventState
    {
        protected StormController stormController;
        public override void OnEnter()
        {
            base.OnEnter();
            this.stormController = base.GetComponent<StormController>();
        }
    }

}