using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemCommando
{
    public sealed class NemCommandoUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.survivor.nemcommando", SS2Bundle.NemCommando);

        public sealed class NemCommandoAchievement : BaseAchievement
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

            private class NemCommandoUnlockableServerAchievement : BaseServerAchievement
            {
                public BodyIndex nemCommandoBodyIndex
                {
                    get
                    {
                        var nemCommandoBodyPrefab = SS2Assets.LoadAsset<GameObject>("NemCommandoBody", SS2Bundle.NemCommando);
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
                    EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemCommandoDefeated;
                }

                public override void OnUninstall()
                {
                    EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal -= OnNemCommandoDefeated;
                    base.OnUninstall();
                }

                private void OnNemCommandoDefeated(CharacterBody obj)
                {
                    if (obj.bodyIndex == nemCommandoBodyIndex)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}