//using EntityStates.Events;
using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Nemmando
{
    public sealed class NemmandoSingleTapUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemmando.singletap", SS2Bundle.Nemmando);

        public override void Initialize()
        {
            AddRequiredType<Survivors.Nemmando>();
        }
        public sealed class NemmandoSingleTapAchievement : BaseAchievement
        {
            public BodyIndex NemmandoBodyIndex
            {
                get
                {
                    var nemmandoBodyPrefab = SS2Assets.LoadAsset<GameObject>("NemmandoBody", SS2Bundle.Nemmando);
                    if (nemmandoBodyPrefab)
                    {
                        return nemmandoBodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                    }
                    return BodyIndex.None;
                }
            }

            public override BodyIndex LookUpRequiredBodyIndex()
            {
                return BodyCatalog.FindBodyIndex("NemmandoBody");
            }

            public override void OnInstall()
            {
                //GenericNemesisEvent.onNemesisDefeatedGlobal += Check;
            }

            private void Check(CharacterBody obj)
            {
                if (obj.bodyIndex == NemmandoBodyIndex && meetsBodyRequirement)
                {
                    Grant();
                }
            }

            public override void OnUninstall()
            {
                //GenericNemesisEvent.onNemesisDefeatedGlobal -= Check;
            }
        }
    }
}