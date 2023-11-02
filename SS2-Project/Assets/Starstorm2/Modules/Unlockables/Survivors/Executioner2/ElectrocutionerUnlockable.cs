using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Executioner2
{
    [DisabledContent]
    public sealed class ElectrocutionerUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.executioner2.electro", SS2Bundle.Executioner2);

        public sealed class ElectrocutionerAchievement : BaseAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                base.SetServerTracked(true);
            }

            public override BodyIndex LookUpRequiredBodyIndex()
            {
                return BodyCatalog.FindBodyIndex("Executioner2Body");
            }

            public override void OnBodyRequirementMet()
            {
                base.OnBodyRequirementMet();
                SetServerTracked(true);
            }

            public override void OnBodyRequirementBroken()
            {
                SetServerTracked(false);
                base.OnBodyRequirementBroken();
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
            }

            private class ElectrocutionerServerAchievement : BaseServerAchievement
            {
                /*public BodyIndex exeuctioner2BodyIndex
                {
                    get
                    {
                        var exe2BodyPrefab = SS2Assets.LoadAsset<GameObject>("Exeuctioner2Body", SS2Bundle.Executioner2);
                        if (exe2BodyPrefab)
                        {
                            return exe2BodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                        }
                        return BodyIndex.None;
                    }
                }*/

                private BodyIndex reminderBodyIndex;

                public override void OnInstall()
                {
                    base.OnInstall();
                    reminderBodyIndex = BodyCatalog.FindBodyIndex("ElectricWormBody");
                    GlobalEventManager.onCharacterDeathGlobal += ElectroTracker;
                }

                private void ElectroTracker(DamageReport damageReport)
                {
                    if (damageReport.victimBodyIndex == reminderBodyIndex && IsCurrentBody(damageReport.attackerBody) && damageReport.damageInfo.damageType == DamageType.BypassOneShotProtection)
                        Grant();
                }

                public override void OnUninstall()
                {
                    GlobalEventManager.onCharacterDeathGlobal -= ElectroTracker;
                    base.OnUninstall();
                }

            }
        }
    }
}