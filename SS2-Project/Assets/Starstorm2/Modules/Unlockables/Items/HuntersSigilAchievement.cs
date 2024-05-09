using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
    public sealed class HuntersSigilAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            GlobalEventManager.onClientDamageNotified += CheckDamage;
        }

        public override void OnUninstall()
        {
            GlobalEventManager.onClientDamageNotified -= CheckDamage;
            base.OnUninstall();
        }
        private void CheckDamage(DamageDealtMessage damageDealtMessage)
        {
            if ((bool)damageDealtMessage.attacker)
            {
                if (damageDealtMessage.attacker == localUser.cachedBodyObject)
                {
                    if (damageDealtMessage.crit && damageDealtMessage.damage >= 1000f)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}