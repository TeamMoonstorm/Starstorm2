/*using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Nemmando
{
    public sealed class NemmandoUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.survivor.nemmando");

        public override void Initialize()
        {
            AddRequiredType<Survivors.Nemmando>();
        }

        public sealed class NemmandoAchievement : BaseAchievement
        {
            public BodyIndex NemmandoBodyIndex
            {
                get
                {
                    var nemmandoBodyPrefab = SS2Assets.LoadAsset<GameObject>("NemmandoBody");
                    if (nemmandoBodyPrefab)
                    {
                        return nemmandoBodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                    }
                    return BodyIndex.None;
                }
            }
            public override void OnInstall()
            {
                base.OnInstall();
                //EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemmandoDefeated;
            }

            public override void OnUninstall()
            {
                //EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal -= OnNemmandoDefeated;
                base.OnUninstall();
            }

            private void OnNemmandoDefeated(CharacterBody obj)
            {
                if (obj.bodyIndex == NemmandoBodyIndex)
                {
                    Grant();
                }
            }
        }
    }
}*/
