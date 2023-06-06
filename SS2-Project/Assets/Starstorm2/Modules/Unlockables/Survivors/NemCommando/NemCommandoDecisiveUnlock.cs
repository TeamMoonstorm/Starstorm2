using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.NemCommando
{
    public sealed class NemCommandoDecisiveUnlock : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemcommando.bossattack", SS2Bundle.NemCommando);

        /*public override void Initialize()
        {
            AddRequiredType<Survivors.Nemmando>();
        }*/

        public sealed class NemCommandoDecisiveAchievement : BaseAchievement
        {
            public override BodyIndex LookUpRequiredBodyIndex()
            {
                return BodyCatalog.FindBodyIndex("NemCommandoBody");
            }

            public override void OnInstall()
            {
                base.OnInstall();
                On.RoR2.CharacterBody.AddBuff_BuffIndex += Check;
            }

            private void Check(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
            {
                if (self)
                {
                    if (self.GetBuffCount(SS2Content.Buffs.BuffGouge) >= 8f && localUser.cachedBody.bodyIndex == requiredBodyIndex)
                    {
                        Grant();
                    }
                }
                orig(self, buffType);
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                On.RoR2.CharacterBody.AddBuff_BuffIndex -= Check;
            }
        }
    }
}