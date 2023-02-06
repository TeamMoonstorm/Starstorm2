using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.Pickups
{
    public sealed class HuntersSigilUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.item.hunterssigil", SS2Bundle.Items);

        public override void Initialize()
        {
            AddRequiredType<Items.HuntersSigil>();
        }
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
}