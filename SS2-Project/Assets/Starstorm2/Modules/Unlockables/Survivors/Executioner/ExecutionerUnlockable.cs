using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.Executioner
{
    public sealed class ExecutionerUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.survivor.executioner", SS2Bundle.Executioner);

        public override void Initialize()
        {
            AddRequiredType<Survivors.Executioner>();
        }
        public sealed class ExecutionerAchievement : BaseAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                GlobalEventManager.onCharacterDeathGlobal += TryUnlock;
            }

            public override void OnUninstall()
            {
                GlobalEventManager.onCharacterDeathGlobal -= TryUnlock;
                base.OnUninstall();
            }

            private void TryUnlock(DamageReport damageReport)
            {
                var attackerMaster = damageReport.attackerMaster;
                if (attackerMaster && attackerMaster.playerCharacterMasterController)
                {
                    var victimBody = damageReport.victimBody;
                    var damage = damageReport.damageDealt;

                    if (victimBody && damage >= victimBody.maxHealth * 10 && victimBody.master)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}