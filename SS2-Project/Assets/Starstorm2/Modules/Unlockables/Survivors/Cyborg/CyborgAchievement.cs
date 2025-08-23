using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Chirr
{
    public sealed class CyborgAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            EtherealBehavior.onEtherealTeleporterChargedGlobal += Check;
        }
        public override void OnUninstall()
        {
            base.OnUninstall();
            EtherealBehavior.onEtherealTeleporterChargedGlobal -= Check;
        }
        private void Check(EtherealBehavior eth)
        {
            Grant();
        }
    }
}