using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.VanillaSurvivors
{
    public sealed class CommandoNemesisSkinUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.commando.nemesisskin", SS2Bundle.Vanilla);

        public sealed class CommandoNemesisSkinAchievement : BaseAchievement
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

            private class CommandoNemesisSkinServerAchievement : BaseServerAchievement
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

                public BodyIndex commandoBodyIndex
                {
                    get
                    {
                        var commandoBodyPrefab = RoR2Content.Survivors.Commando.bodyPrefab;
                        if (commandoBodyPrefab)
                        {
                            return commandoBodyPrefab.GetComponent<CharacterBody>().bodyIndex;
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
                    if (obj.bodyIndex == nemCommandoBodyIndex && networkUser.GetCurrentBody().bodyIndex == commandoBodyIndex)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}