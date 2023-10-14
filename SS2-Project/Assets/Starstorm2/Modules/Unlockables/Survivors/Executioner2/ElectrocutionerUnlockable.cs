using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Executioner2
{
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

            public override void OnUninstall()
            {
                base.OnUninstall();
            }

            private class ElectrocutionerServerAchievement : BaseServerAchievement
            {
                public BodyIndex exeuctioner2BodyIndex
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
                }
                public override void OnInstall()
                {
                    base.OnInstall();
                    EntityStates.Events.OverloadingEventState.onEventClearGlobal += OnEventCleared;
                }

                public override void OnUninstall()
                {
                    EntityStates.Events.OverloadingEventState.onEventClearGlobal -= OnEventCleared;
                    base.OnUninstall();
                }

                private void OnEventCleared()
                {
                    if (networkUser.GetCurrentBody().bodyIndex == exeuctioner2BodyIndex)
                        Grant();
                }
            }
        }
    }
}