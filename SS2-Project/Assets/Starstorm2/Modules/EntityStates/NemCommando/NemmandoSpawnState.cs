using EntityStates.Generic;

namespace EntityStates.Nemmando
{
    public class NemmandoSpawnState : NemesisSpawnState
    {
        public override void SpawnEffect()
        {
            portalMuzzle = "PortalSpawn";
            base.SpawnEffect();
        }
    }
}