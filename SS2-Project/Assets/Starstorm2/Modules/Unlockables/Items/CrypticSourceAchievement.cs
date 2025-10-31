using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
    public sealed class CrypticSourceAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
        }
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            EtherealBehavior.onEtherealTeleporterChargedGlobal += Check;
        }
        private void Check(EtherealBehavior eth)
        {
            if(eth.etherealsCompleted >= 2) // etherealsCompleted doesnt increase until the stage after completing tp. so this means 3 total
            {
                Grant();
            }
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            EtherealBehavior.onEtherealTeleporterChargedGlobal -= Check;
        }
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("MercBody");
        }
    }
}