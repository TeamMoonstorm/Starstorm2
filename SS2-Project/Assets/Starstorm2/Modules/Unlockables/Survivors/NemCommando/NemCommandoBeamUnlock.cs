using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.NemCommando
{
    public sealed class NemCommandoBeamUnlock : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemcommando.swordbeam", SS2Bundle.NemCommando);

        /*public override void Initialize()
        {
            AddRequiredType<Survivors.Nemmando>();
        }*/

        public sealed class NemCommandoBeamAchievement : BaseAchievement
        {
            public override BodyIndex LookUpRequiredBodyIndex()
            {
                return BodyCatalog.FindBodyIndex("NemCommandoBody");
            }

            public override void OnInstall()
            {
                base.OnInstall();
                RoR2Application.onUpdate += CheckBleedChance;
            }


            public override void OnUninstall()
            {
                RoR2Application.onUpdate -= CheckBleedChance;
                base.OnUninstall();
            }

            private void CheckBleedChance()
            {
                if (localUser != null && (bool)localUser.cachedBody)
                {
                    if (localUser.cachedBody.bleedChance >= 100f && localUser.cachedBody.bodyIndex == requiredBodyIndex)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}