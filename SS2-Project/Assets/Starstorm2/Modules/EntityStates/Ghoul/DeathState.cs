namespace EntityStates.Ghoul
{
    public class DeathState : GenericCharacterDeath
    {
        public override bool shouldAutoDestroy => true;

        public override void OnEnter()
        {
            base.OnEnter();

            if (this.cachedModelTransform)
            {
                EntityState.Destroy(this.cachedModelTransform.gameObject);
                this.cachedModelTransform = null;
            }
        }
    }
}
