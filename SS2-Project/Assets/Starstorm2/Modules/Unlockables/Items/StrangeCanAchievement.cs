using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
    public sealed class StrangeCanAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            GlobalEventManager.onServerDamageDealt += CheckDebuff;
        }

        public override void OnUninstall()
        {
            GlobalEventManager.onServerDamageDealt -= CheckDebuff;
            base.OnUninstall();
        }
        private void CheckDebuff(DamageReport damageReport)
        {
            var victimBody = damageReport.victimBody;
            var attackerBody = damageReport.attackerBody;

            var debuffAmount = 0;

            BuffIndex[] debuffBuffIndices = BuffCatalog.debuffBuffIndices;
            foreach (BuffIndex buffType in debuffBuffIndices)
            {
                if (victimBody.HasBuff(buffType))
                {
                    debuffAmount++;
                }
            }
            if (debuffAmount >= 6)
            {
                Grant();
            }
        }
    }
}