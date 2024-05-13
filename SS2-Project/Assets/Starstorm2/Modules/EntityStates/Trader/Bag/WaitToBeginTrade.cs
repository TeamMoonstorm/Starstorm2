namespace EntityStates.Trader.Bag
{
    public class WaitToBeginTrade : BagBaseState
    {
        public static float duration;
        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            // TODO: Swuff doesnt know why this is here, but ill leave the code if it comes back to swuff, delete this if you dont need it please
            // if (isAuthority)
            //     characterBody.SetBuffCount(SS2.SS2Content.Buffs.bdHiddenSlow20.buffIndex, 10);
            PlayCrossfade("Body", "Scavenge", "Scavenge.playbackRate", duration + Trading.duration + TradeToIdle.duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
                outer.SetNextState(new Trading());
        }
    }
}
