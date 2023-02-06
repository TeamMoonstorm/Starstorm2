using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.Pickups
{
    public sealed class StrangeCanUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.item.strangecan", SS2Bundle.Indev);

        public override void Initialize()
        {
            AddRequiredType<Items.StrangeCan>();
        }
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
}