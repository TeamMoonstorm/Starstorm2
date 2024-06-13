using SS2.Components;
using RoR2;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using SS2;
namespace EntityStates.Events
{
    public abstract class GenericWeatherState : EntityState
    {
        protected GameplayEvent gameplayEvent;
        protected StormController stormController;
        public override void OnEnter()
        {
            base.OnEnter();
            this.gameplayEvent = base.GetComponent<GameplayEvent>();
            this.stormController = base.GetComponent<StormController>();
        }
    }

}