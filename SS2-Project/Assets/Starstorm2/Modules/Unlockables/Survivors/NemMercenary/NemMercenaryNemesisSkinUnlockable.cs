using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryNemesisSkinUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.nemmerc.nemesisskin", SS2Bundle.NemMercenary);

        public sealed class NemMercenaryNemesisSkinAchievement : BaseAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                SetServerTracked(true);
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
            }

            private class NemMercenaryNemesisSkinServerAchievement : BaseServerAchievement
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
                    if (obj.bodyIndex == nemMercBodyIndex && networkUser.GetCurrentBody().bodyIndex == nemMercBodyIndex)
                    {
                        Grant();
                    }
                }

            }
        }
    }
}