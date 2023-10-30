using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.survivor.nemmerc", SS2Bundle.NemMercenary);

        public sealed class NemMercenaryAchievement : BaseAchievement
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

            private class NemMercenaryUnlockableServerAchievement : BaseServerAchievement
            {
                public BodyIndex nemMercBodyIndex
                {
                    get
                    {
                        var nemCommandoBodyPrefab = SS2Assets.LoadAsset<GameObject>("NemMercBody", SS2Bundle.NemMercenary);
                        if (nemCommandoBodyPrefab)
                        {
                            return nemCommandoBodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                        }
                        return BodyIndex.None;
                    }
                }
                public override void OnInstall()
                {
                    base.OnInstall();
                    EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemMercenaryDefeated;
                }

                public override void OnUninstall()
                {
                    EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal -= OnNemMercenaryDefeated;
                    base.OnUninstall();
                }

                private void OnNemMercenaryDefeated(CharacterBody obj)
                {
                    if (obj.bodyIndex == nemMercBodyIndex)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}