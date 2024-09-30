namespace EntityStates.NemMerc.Clone
{
    public class CloneDeath : GenericCharacterDeath
    {
        /// <summary>
        /// // IDK DO SOMETHING COOL HERE
        /// </summary>
        /// 

        public override void OnEnter()
        {
            base.OnEnter();

            DestroyModel();
        }
        public override bool shouldAutoDestroy => true;
    }
}
