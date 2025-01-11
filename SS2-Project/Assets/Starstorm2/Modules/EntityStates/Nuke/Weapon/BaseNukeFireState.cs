namespace EntityStates.Nuke
{
    /// <summary>
    /// A Custom <see cref="BaseState"/> that implements <see cref="SS2.Survivors.Nuke.IChargedState"/>. Most of the time this state is set right after a <see cref="BaseNukeChargeState"/> exits
    /// </summary>
    public abstract class BaseNukeFireState : BaseState, SS2.Survivors.Nuke.IChargedState
    {
        public float charge { get; set; }

        /// <summary>
        /// Adds the SpreadBloom to the crosshair
        /// </summary>
        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.AddSpreadBloom(charge);
        }
    }
}