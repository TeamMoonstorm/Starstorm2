using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryKnifeUnlock : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemmercenary.knife", SS2Bundle.NemMercenary);

        public sealed class NemMercenaryKnifeAchievement : BaseAchievement
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
                if (obj.bodyIndex != nemMercBodyIndex && localUser.cachedBody.bodyIndex == nemMercBodyIndex)
                {
                    Grant();
                }
            }
        }
    }
}