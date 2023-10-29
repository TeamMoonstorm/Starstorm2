using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.VanillaSurvivors
{
    public sealed class MercenaryNemesisSkinUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.mercenary.nemesisskin", SS2Bundle.Vanilla);

        public sealed class MercenaryNemesisSkinAchievement : BaseAchievement
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

            private class MercenaryNemesisSkinServerAchievement : BaseServerAchievement
            {
                public BodyIndex nemMercenaryBodyIndex
                {
                    get
                    {
                        var nemMercenaryBodyPrefab = SS2Assets.LoadAsset<GameObject>("NemMercBody", SS2Bundle.NemMercenary);
                        if (nemMercenaryBodyPrefab)
                        {
                            return nemMercenaryBodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                        }
                        return BodyIndex.None;
                    }
                }

                public BodyIndex mercenaryBodyIndex
                {
                    get
                    {
                        var mercenaryBodyPrefab = RoR2Content.Survivors.Merc.bodyPrefab;
                        if (mercenaryBodyPrefab)
                        {
                            return mercenaryBodyPrefab.GetComponent<CharacterBody>().bodyIndex;
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
                    if (obj.bodyIndex == nemMercenaryBodyIndex && networkUser.GetCurrentBody().bodyIndex == mercenaryBodyIndex)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}